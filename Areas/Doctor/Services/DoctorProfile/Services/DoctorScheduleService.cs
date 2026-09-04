using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Wellora.Areas.Doctor.Models;
using Wellora.Areas.Doctor.Services.DoctorProfile.Interfaces;
using Wellora.Areas.Doctor.ViewModels.DoctorProfile;
using Wellora.Data;

namespace Wellora.Areas.Doctor.Services.DoctorProfile.Services
{
    public class DoctorScheduleService : IDoctorScheduleService
    {
        private readonly ApplicationDbContext _context;

        private const int MaximumMonthlyChanges = 6;
        private const int MinimumShiftHours = 2;
        private const int MaximumShiftHours = 18;
        private const int MinimumBreakShiftHours = 4;
        private const int MaximumBreakHours = 3;
        private const int MinimumBreakAfterShiftStartHours = 1;
        private const int ScheduleActivationBufferDays = 7;

        public DoctorScheduleService(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // GET CURRENT SCHEDULE
        // =========================================================

        public async Task<DoctorScheduleViewModel> GetScheduleAsync(
            int doctorId,
            CancellationToken cancellationToken)
        {
            var schedules = await _context.DoctorSchedules
                .AsNoTracking()
                .Where(s => s.DoctorId == doctorId)
                .OrderBy(s => s.DayOfWeek)
                .ToListAsync(cancellationToken);

            var breaks = await _context.DoctorBreaks
                .AsNoTracking()
                .Where(b => b.DoctorId == doctorId)
                .OrderBy(b => b.DayOfWeek)
                .ToListAsync(cancellationToken);

            var model = new DoctorScheduleViewModel
            {
                DoctorId = doctorId
            };

            foreach (var schedule in schedules)
            {
                model.ScheduleRows.Add(new DoctorScheduleRow
                {
                    ScheduleId = schedule.ScheduleId,
                    DayOfWeek = schedule.DayOfWeek,

                    StartTime = schedule.StartTime,
                    EndTime = schedule.EndTime,

                    AppointmentDurationMin =
                        schedule.AppointmentDurationMin,

                    MaxPatientsPerDay =
                        schedule.MaxPatientsPerDay,

                    BufferTimeMin =
                        schedule.BufferTimeMin,

                    // A record existing in doctors_schedule
                    // means this day is ON.
                    Status = true
                });
            }

            foreach (var item in breaks)
            {
                model.Breaks.Add(new DoctorBreakViewModel
                {
                    BreakId = item.BreakId,
                    DoctorId = item.DoctorId,
                    DayOfWeek = item.DayOfWeek,
                    BreakStart = item.BreakStart,
                    BreakEnd = item.BreakEnd
                });
            }

            return model;
        }


        // =========================================================
        // UPDATE SCHEDULE
        // =========================================================

        public async Task<ScheduleUpdateResult> UpdateScheduleAsync(
            int doctorId,
            int userId,
            DoctorScheduleUpdateViewModel model,
            CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var today = DateOnly.FromDateTime(now);

            // -----------------------------------------------------
            // 1. Validate doctor
            // -----------------------------------------------------

            var doctorExists = await _context.Doctors
                .AnyAsync(
                    d => d.DoctorId == doctorId,
                    cancellationToken);

            if (!doctorExists)
            {
                return CreateFailureResult(
                    "Doctor profile was not found.");
            }


            // -----------------------------------------------------
            // 2. Validate model
            // -----------------------------------------------------

            if (model == null)
            {
                return CreateFailureResult(
                    "Invalid schedule data.");
            }

            if (model.ScheduleRows == null)
            {
                return CreateFailureResult(
                    "Schedule data was not provided.");
            }


            // -----------------------------------------------------
            // 3. Check monthly change limit
            // -----------------------------------------------------

            var monthStart = new DateTime(
                now.Year,
                now.Month,
                1);

            var nextMonth = monthStart.AddMonths(1);

            var changesThisMonth =
                await _context.DoctorScheduleHistorys
                    .CountAsync(
                        h =>
                            h.DoctorId == doctorId &&
                            h.ChangedAt >= monthStart &&
                            h.ChangedAt < nextMonth &&
                            h.Status != "CANCELLED",
                        cancellationToken);

            if (changesThisMonth >= MaximumMonthlyChanges)
            {
                return new ScheduleUpdateResult
                {
                    Success = false,
                    Message =
                        "You have reached the maximum of 6 schedule changes for this month.",
                    ChangesThisMonth = changesThisMonth,
                    RemainingChanges = 0
                };
            }


            // -----------------------------------------------------
            // 4. Validate schedule rules
            // -----------------------------------------------------

            var validationResult =
                ValidateSchedule(model);

            if (!validationResult.Success)
            {
                return new ScheduleUpdateResult
                {
                    Success = false,
                    Message = validationResult.Message,
                    ChangesThisMonth = changesThisMonth,
                    RemainingChanges =
                        Math.Max(
                            0,
                            MaximumMonthlyChanges - changesThisMonth)
                };
            }


            // -----------------------------------------------------
            // 5. Get current database schedule
            // -----------------------------------------------------

            var currentSchedules =
                await _context.DoctorSchedules
                    .Where(s => s.DoctorId == doctorId)
                    .ToListAsync(cancellationToken);

            var currentBreaks =
                await _context.DoctorBreaks
                    .Where(b => b.DoctorId == doctorId)
                    .ToListAsync(cancellationToken);


            // -----------------------------------------------------
            // 6. Find previous pending schedule
            //
            // If the doctor changes the schedule again before the
            // previous schedule becomes active, the previous pending
            // schedule is replaced.
            // -----------------------------------------------------

            var previousPending =
                await _context.DoctorScheduleHistorys
                    .Where(h =>
                        h.DoctorId == doctorId &&
                        h.Status == "PENDING")
                    .OrderByDescending(h => h.ChangedAt)
                    .FirstOrDefaultAsync(cancellationToken);


            // -----------------------------------------------------
            // 7. Create old schedule snapshot
            // -----------------------------------------------------

            string? oldScheduleJson = null;

            if (currentSchedules.Any() || currentBreaks.Any())
            {
                oldScheduleJson =
                    JsonSerializer.Serialize(
                        CreateScheduleSnapshot(
                            currentSchedules,
                            currentBreaks));
            }
            else if (previousPending != null &&
                     !string.IsNullOrWhiteSpace(
                         previousPending.NewSchedule))
            {
                // If the current database is empty because a pending
                // schedule was already removed, preserve the previous
                // pending schedule as the "old" schedule.
                oldScheduleJson =
                    previousPending.NewSchedule;
            }


            // -----------------------------------------------------
            // 8. Build new schedule
            // -----------------------------------------------------

            var newSchedules =
                model.ScheduleRows
                    .Where(r => r.Status == true)
                    .Select(r => new ScheduleSnapshotRow
                    {
                        DayOfWeek = r.DayOfWeek,

                        StartTime = r.StartTime!.Value,

                        EndTime = r.EndTime!.Value,

                        AppointmentDurationMin =
                            r.AppointmentDurationMin ?? 30,

                        MaxPatientsPerDay =
                            r.MaxPatientsPerDay ?? 1,

                        BufferTimeMin =
                            r.BufferTimeMin ?? 0
                    })
                    .ToList();


            // -----------------------------------------------------
            // 9. Build new breaks
            // -----------------------------------------------------

            var newBreaks =
                model.Breaks
                    .Where(b =>
                        b.BreakStart.HasValue &&
                        b.BreakEnd.HasValue)
                    .Select(b => new BreakSnapshotRow
                    {
                        DayOfWeek = b.DayOfWeek,

                        BreakStart =
                            b.BreakStart!.Value,

                        BreakEnd =
                            b.BreakEnd!.Value
                    })
                    .ToList();


            // -----------------------------------------------------
            // 10. Create new JSON snapshot
            // -----------------------------------------------------

            var newSnapshot = new ScheduleSnapshot
            {
                Schedules = newSchedules,
                Breaks = newBreaks
            };

            var newScheduleJson =
                JsonSerializer.Serialize(newSnapshot);


            // -----------------------------------------------------
            // 11. Find last appointment
            // -----------------------------------------------------

            var lastAppointment =
                await _context.Appointments
                    .Where(a =>
                        a.DoctorId == doctorId)
                    .MaxAsync(
                        a => (DateTime?)a.AppointmentDate,
                        cancellationToken);


            // -----------------------------------------------------
            // 12. Calculate effective date
            // -----------------------------------------------------

            DateOnly effectiveFrom;

            if (lastAppointment.HasValue)
            {
                effectiveFrom =
                    DateOnly.FromDateTime(
                        lastAppointment.Value.Date
                            .AddDays(ScheduleActivationBufferDays));
            }
            else
            {
                effectiveFrom = today;
            }


            // -----------------------------------------------------
            // 13. Cancel previous pending schedules
            // -----------------------------------------------------

            var pendingSchedules =
                await _context.DoctorScheduleHistorys
                    .Where(h =>
                        h.DoctorId == doctorId &&
                        h.Status == "PENDING")
                    .ToListAsync(cancellationToken);

            foreach (var pending in pendingSchedules)
            {
                pending.Status = "CANCELLED";
            }


            // -----------------------------------------------------
            // 14. Remove current schedule
            //
            // The scheduler will see no schedule while the new
            // schedule is waiting for its effective date.
            // -----------------------------------------------------

            _context.DoctorSchedules.RemoveRange(
                currentSchedules);

            _context.DoctorBreaks.RemoveRange(
                currentBreaks);


            // -----------------------------------------------------
            // 15. Determine action type
            // -----------------------------------------------------

            string actionType;

            if (currentSchedules.Any() ||
                currentBreaks.Any() ||
                previousPending != null)
            {
                actionType = "UPDATE";
            }
            else
            {
                actionType = "INSERT";
            }


            // -----------------------------------------------------
            // 16. Determine status
            // -----------------------------------------------------

            var historyStatus =
                effectiveFrom > today
                    ? "PENDING"
                    : "COMPLETED";


            // -----------------------------------------------------
            // 17. Create history
            // -----------------------------------------------------

            var history = new DoctorScheduleHistory
            {
                DoctorId = doctorId,

                ChangedByUserId = userId,

                ChangedAt = now,

                ActionType = actionType,

                OldSchedule = oldScheduleJson,

                NewSchedule = newScheduleJson,

                EffectiveFrom = effectiveFrom,

                Status = historyStatus
            };

            _context.DoctorScheduleHistorys.Add(history);


            // -----------------------------------------------------
            // 18. If immediately effective, save current schedule
            // -----------------------------------------------------

            if (historyStatus == "COMPLETED")
            {
                AddCurrentSchedule(
                    doctorId,
                    newSchedules,
                    newBreaks);
            }


            // -----------------------------------------------------
            // 19. Save transaction
            // -----------------------------------------------------

            await _context.SaveChangesAsync(
                cancellationToken);


            // -----------------------------------------------------
            // 20. Return result
            // -----------------------------------------------------

            var newChangeCount =
                changesThisMonth + 1;

            return new ScheduleUpdateResult
            {
                Success = true,

                Message =
                    historyStatus == "PENDING"
                        ? $"Schedule saved. It will become active on {effectiveFrom:dd MMM yyyy}."
                        : "Schedule updated successfully.",

                EffectiveFrom = effectiveFrom,

                ChangesThisMonth = newChangeCount,

                RemainingChanges =
                    Math.Max(
                        0,
                        MaximumMonthlyChanges -
                        newChangeCount)
            };
        }


        // =========================================================
        // ACTIVATE ALL PENDING SCHEDULES
        // =========================================================

        public async Task ActivatePendingSchedulesAsync(
            CancellationToken cancellationToken)
        {
            var today =
                DateOnly.FromDateTime(
                    DateTime.UtcNow);


            // -----------------------------------------------------
            // Get all schedules that should now become active.
            // -----------------------------------------------------

            var pendingSchedules =
                await _context.DoctorScheduleHistorys
                    .Where(h =>
                        h.Status == "PENDING" &&
                        h.EffectiveFrom.HasValue &&
                        h.EffectiveFrom.Value <= today)
                    .OrderBy(h => h.EffectiveFrom)
                    .ThenBy(h => h.ChangedAt)
                    .ToListAsync(cancellationToken);


            foreach (var pending in pendingSchedules)
            {
                if (string.IsNullOrWhiteSpace(
                    pending.NewSchedule))
                {
                    pending.Status = "CANCELLED";
                    continue;
                }


                var snapshot =
                    JsonSerializer.Deserialize<ScheduleSnapshot>(
                        pending.NewSchedule);


                if (snapshot == null)
                {
                    pending.Status = "CANCELLED";
                    continue;
                }


                // -------------------------------------------------
                // Remove existing schedule
                // -------------------------------------------------

                var currentSchedules =
                    await _context.DoctorSchedules
                        .Where(s =>
                            s.DoctorId ==
                            pending.DoctorId)
                        .ToListAsync(
                            cancellationToken);

                var currentBreaks =
                    await _context.DoctorBreaks
                        .Where(b =>
                            b.DoctorId ==
                            pending.DoctorId)
                        .ToListAsync(
                            cancellationToken);


                _context.DoctorSchedules
                    .RemoveRange(currentSchedules);

                _context.DoctorBreaks
                    .RemoveRange(currentBreaks);


                // -------------------------------------------------
                // Insert new schedule
                // -------------------------------------------------

                AddCurrentSchedule(
                    pending.DoctorId,
                    snapshot.Schedules,
                    snapshot.Breaks);


                // -------------------------------------------------
                // Mark history completed
                // -------------------------------------------------

                pending.Status = "COMPLETED";
            }


            await _context.SaveChangesAsync(
                cancellationToken);
        }


        // =========================================================
        // VALIDATE SCHEDULE
        // =========================================================

        private static (bool Success, string Message)
            ValidateSchedule(
                DoctorScheduleUpdateViewModel model)
        {
            // -----------------------------------------------------
            // Only ON rows are saved.
            //
            // false = OFF
            // null  = -
            // -----------------------------------------------------

            var enabledRows =
                model.ScheduleRows
                    .Where(r => r.Status == true)
                    .ToList();


            // -----------------------------------------------------
            // Validate day numbers
            // -----------------------------------------------------

            foreach (var row in model.ScheduleRows)
            {
                if (row.DayOfWeek < 1 ||
                    row.DayOfWeek > 7)
                {
                    return (
                        false,
                        "Invalid day of week.");
                }
            }


            // -----------------------------------------------------
            // Validate duplicate days
            // -----------------------------------------------------

            var duplicateDays =
                model.ScheduleRows
                    .GroupBy(r => r.DayOfWeek)
                    .Any(g => g.Count() > 1);

            if (duplicateDays)
            {
                return (
                    false,
                    "A day cannot appear more than once.");
            }


            // -----------------------------------------------------
            // Validate ON rows
            // -----------------------------------------------------

            foreach (var row in enabledRows)
            {
                if (!row.StartTime.HasValue ||
                    !row.EndTime.HasValue)
                {
                    return (
                        false,
                        $"Please select start and end time for day {row.DayOfWeek}.");
                }


                if (row.EndTime.Value <=
                    row.StartTime.Value)
                {
                    return (
                        false,
                        $"End time must be after start time for day {row.DayOfWeek}.");
                }


                var shiftDuration =
                    row.EndTime.Value -
                    row.StartTime.Value;


                // -------------------------------------------------
                // Minimum shift = 2 hours
                // -------------------------------------------------

                if (shiftDuration.TotalHours <
                    MinimumShiftHours)
                {
                    return (
                        false,
                        $"The shift for day {row.DayOfWeek} must be at least 2 hours.");
                }


                // -------------------------------------------------
                // Maximum shift = 18 hours
                // -------------------------------------------------

                if (shiftDuration.TotalHours >
                    MaximumShiftHours)
                {
                    return (
                        false,
                        $"The shift for day {row.DayOfWeek} cannot exceed 18 hours.");
                }


                // -------------------------------------------------
                // Defaults
                // -------------------------------------------------

                var appointmentDuration =
                    row.AppointmentDurationMin ?? 30;

                var maxPatients =
                    row.MaxPatientsPerDay ?? 1;

                var buffer =
                    row.BufferTimeMin ?? 0;


                // -------------------------------------------------
                // Patient duration
                // -------------------------------------------------

                if (appointmentDuration <= 0)
                {
                    return (
                        false,
                        $"Patient duration must be greater than 0 for day {row.DayOfWeek}.");
                }


                // -------------------------------------------------
                // Maximum patients
                // -------------------------------------------------

                if (maxPatients < 1)
                {
                    return (
                        false,
                        $"Maximum patients must be at least 1 for day {row.DayOfWeek}.");
                }


                // -------------------------------------------------
                // Buffer
                // -------------------------------------------------

                if (buffer < 0)
                {
                    return (
                        false,
                        $"Buffer time cannot be negative for day {row.DayOfWeek}.");
                }


                // -------------------------------------------------
                // Capacity calculation
                // -------------------------------------------------

                var availableMinutes =
                    (int)shiftDuration.TotalMinutes;

                var slotMinutes =
                    appointmentDuration +
                    buffer;


                if (slotMinutes <= 0)
                {
                    return (
                        false,
                        $"Invalid appointment duration or buffer for day {row.DayOfWeek}.");
                }


                var maximumPossiblePatients =
                    availableMinutes /
                    slotMinutes;


                if (maxPatients >
                    maximumPossiblePatients)
                {
                    return (
                        false,
                        $"Maximum patients for day {row.DayOfWeek} exceeds the available appointment capacity.");
                }
            }


            // -----------------------------------------------------
            // Validate breaks
            // -----------------------------------------------------

            foreach (var breakRow in model.Breaks)
            {
                var hasStart =
                    breakRow.BreakStart.HasValue;

                var hasEnd =
                    breakRow.BreakEnd.HasValue;


                // No break = don't save anything.
                if (!hasStart && !hasEnd)
                {
                    continue;
                }


                // One selected but not the other.
                if (!hasStart || !hasEnd)
                {
                    return (
                        false,
                        $"Both break start and break end are required for day {breakRow.DayOfWeek}.");
                }


                // Break only allowed for ON days.
                var schedule =
                    enabledRows.FirstOrDefault(
                        s =>
                            s.DayOfWeek ==
                            breakRow.DayOfWeek);


                if (schedule == null)
                {
                    return (
                        false,
                        $"A break cannot be configured for day {breakRow.DayOfWeek} because that day is not scheduled.");
                }


                // -------------------------------------------------
                // Shift duration
                // -------------------------------------------------

                var shiftDuration =
                    schedule.EndTime!.Value -
                    schedule.StartTime!.Value;


                // Breaks require a minimum 4-hour shift.
                if (shiftDuration.TotalHours <
                    MinimumBreakShiftHours)
                {
                    return (
                        false,
                        $"A break is only allowed for shifts of at least 4 hours.");
                }


                // -------------------------------------------------
                // Break duration
                // -------------------------------------------------

                var breakDuration =
                    breakRow.BreakEnd!.Value -
                    breakRow.BreakStart!.Value;


                if (breakDuration <=
                    TimeSpan.Zero)
                {
                    return (
                        false,
                        $"Break end must be after break start for day {breakRow.DayOfWeek}.");
                }


                // Maximum break = 3 hours.
                if (breakDuration.TotalHours >
                    MaximumBreakHours)
                {
                    return (
                        false,
                        $"A break cannot be longer than 3 hours.");
                }


                // -------------------------------------------------
                // Break must start at least 1 hour after shift
                // begins.
                // -------------------------------------------------

                var earliestBreakStart =
                    schedule.StartTime.Value.Add(
                        TimeSpan.FromHours(
                            MinimumBreakAfterShiftStartHours));


                if (breakRow.BreakStart.Value <
                    earliestBreakStart)
                {
                    return (
                        false,
                        $"The break on day {breakRow.DayOfWeek} must start at least 1 hour after the shift begins.");
                }


                // -------------------------------------------------
                // Break must finish before shift ends.
                // -------------------------------------------------

                if (breakRow.BreakEnd.Value >
                    schedule.EndTime.Value)
                {
                    return (
                        false,
                        $"The break on day {breakRow.DayOfWeek} must finish before the shift ends.");
                }
            }


            return (
                true,
                string.Empty);
        }


        // =========================================================
        // CREATE SNAPSHOT FROM DATABASE
        // =========================================================

        private static ScheduleSnapshot CreateScheduleSnapshot(
            IEnumerable<DoctorSchedule> schedules,
            IEnumerable<DoctorBreak> breaks)
        {
            return new ScheduleSnapshot
            {
                Schedules =
                    schedules
                        .Select(s => new ScheduleSnapshotRow
                        {
                            DayOfWeek = s.DayOfWeek,

                            StartTime = s.StartTime,

                            EndTime = s.EndTime,

                            AppointmentDurationMin =
                                s.AppointmentDurationMin,

                            MaxPatientsPerDay =
                                s.MaxPatientsPerDay,

                            BufferTimeMin =
                                s.BufferTimeMin
                        })
                        .ToList(),

                Breaks =
                    breaks
                        .Select(b => new BreakSnapshotRow
                        {
                            DayOfWeek = b.DayOfWeek,

                            BreakStart = b.BreakStart,

                            BreakEnd = b.BreakEnd
                        })
                        .ToList()
            };
        }


        // =========================================================
        // ADD CURRENT SCHEDULE
        // =========================================================

        private void AddCurrentSchedule(
            int doctorId,
            IEnumerable<ScheduleSnapshotRow> schedules,
            IEnumerable<BreakSnapshotRow> breaks)
        {
            foreach (var schedule in schedules)
            {
                _context.DoctorSchedules.Add(
                    new DoctorSchedule
                    {
                        DoctorId = doctorId,

                        DayOfWeek =
                            schedule.DayOfWeek,

                        StartTime =
                            schedule.StartTime,

                        EndTime =
                            schedule.EndTime,

                        AppointmentDurationMin =
                            schedule.AppointmentDurationMin,

                        MaxPatientsPerDay =
                            schedule.MaxPatientsPerDay,

                        BufferTimeMin =
                            schedule.BufferTimeMin
                    });
            }


            foreach (var item in breaks)
            {
                _context.DoctorBreaks.Add(
                    new DoctorBreak
                    {
                        DoctorId = doctorId,

                        DayOfWeek =
                            item.DayOfWeek,

                        BreakStart =
                            item.BreakStart,

                        BreakEnd =
                            item.BreakEnd
                    });
            }
        }


        // =========================================================
        // FAILURE RESULT
        // =========================================================

        private static ScheduleUpdateResult CreateFailureResult(
            string message)
        {
            return new ScheduleUpdateResult
            {
                Success = false,

                Message = message,

                EffectiveFrom = null,

                ChangesThisMonth = 0,

                RemainingChanges =
                    MaximumMonthlyChanges
            };
        }
    }


    // =============================================================
    // JSON SNAPSHOT
    // =============================================================

    public class ScheduleSnapshot
    {
        public List<ScheduleSnapshotRow> Schedules { get; set; }
            = new();

        public List<BreakSnapshotRow> Breaks { get; set; }
            = new();
    }


    public class ScheduleSnapshotRow
    {
        public int DayOfWeek { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public int AppointmentDurationMin { get; set; }

        public int MaxPatientsPerDay { get; set; }

        public int BufferTimeMin { get; set; }
    }


    public class BreakSnapshotRow
    {
        public int DayOfWeek { get; set; }

        public TimeSpan BreakStart { get; set; }

        public TimeSpan BreakEnd { get; set; }
    }
}
