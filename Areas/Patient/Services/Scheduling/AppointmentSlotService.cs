using Wellora.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using Wellora.Areas.Doctor.Models;
using Wellora.Areas.Patient.ViewModels.MakeAppointment;

namespace Wellora.Areas.Patient.Services.Scheduling
{
    public class AppointmentSlotService
    {
        private readonly ApplicationDbContext _context;

        public AppointmentSlotService(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // MAIN ENTRY POINT
        // =========================
        public (List<SlotItem?> Morning,
                List<SlotItem?> Afternoon,
                List<SlotItem?> Evening,
                bool NoSlots)
        GenerateSlots(int doctorId, DateTime date)
        {
            int dbDay = (int)date.DayOfWeek == 0 ? 7 : (int)date.DayOfWeek;

            var schedule = _context.DoctorSchedules
                .FirstOrDefault(s => s.DoctorId == doctorId
                                  && s.DayOfWeek == dbDay
                                  && s.IsActive);

            if (schedule == null)
            {
                return EmptyResult();
            }

            var breaks = _context.DoctorBreaks
                .Where(b => b.DoctorId == doctorId && b.DayOfWeek == dbDay)
                .ToList();

            var booked = _context.Appointments
                .Where(a => a.DoctorId == doctorId
                         && a.AppointmentDate.Date == date.Date
                         && a.Status != "cancelled")
                .Select(a => a.AppointmentDate)
                .ToList();

            var duration = TimeSpan.FromMinutes(schedule.AppointmentDurationMin);
            var buffer = TimeSpan.FromMinutes(schedule.BufferTimeMin);

            var start = date.Date + schedule.StartTime;
            var end = date.Date + schedule.EndTime;

            var morning = new List<SlotItem>();
            var afternoon = new List<SlotItem>();
            var evening = new List<SlotItem>();

            var now = DateTime.Now;

            while (start + duration <= end)
            {
                var slotStart = start;
                var slotEnd = start + duration;

                bool isBreak = breaks.Any(br =>
                    slotStart < date.Date + br.BreakEnd &&
                    slotEnd > date.Date + br.BreakStart);

                bool isBooked = booked.Any(b => b == slotStart);

                bool isPast = (date.Date == now.Date && slotStart <= now);

                bool isAvailable = !isBreak && !isBooked && !isPast;

                var slot = new SlotItem
                {
                    Time = slotStart.ToString("hh:mm tt"),
                    IsAvailable = isAvailable
                };

                AssignToColumn(slotStart, slot, morning, afternoon, evening);

                start = slotEnd + buffer;
            }

            var m = Pad(morning);
            var a = Pad(afternoon);
            var e = Pad(evening);

            bool noSlots =
                !m.Any(x => x?.IsAvailable == true) &&
                !a.Any(x => x?.IsAvailable == true) &&
                !e.Any(x => x?.IsAvailable == true);

            return (m, a, e, noSlots);
        }

        // =========================
        // TIME COLUMN RULES
        // =========================
        private void AssignToColumn(
            DateTime time,
            SlotItem slot,
            List<SlotItem> morning,
            List<SlotItem> afternoon,
            List<SlotItem> evening)
        {
            int hour = time.Hour;

            // Morning: 03:00 - 11:59
            if (hour >= 3 && hour < 12)
            {
                if (morning.Count < 10)
                    morning.Add(slot);
            }
            // Afternoon: 12:00 - 16:59
            else if (hour >= 12 && hour < 17)
            {
                if (afternoon.Count < 10)
                    afternoon.Add(slot);
            }
            // Evening: 17:00 - 02:59
            else
            {
                if (evening.Count < 10)
                    evening.Add(slot);
            }
        }

        // =========================
        // FIXED GRID (10 SLOTS)
        // =========================
        private List<SlotItem?> Pad(List<SlotItem> list)
        {
            var result = list.Take(10).Cast<SlotItem?>().ToList();

            while (result.Count < 10)
                result.Add(null);

            return result;
        }

        // =========================
        // EMPTY FALLBACK
        // =========================
        private (List<SlotItem?>, List<SlotItem?>, List<SlotItem?>, bool)
        EmptyResult()
        {
            return (
                Enumerable.Repeat<SlotItem?>(null, 10).ToList(),
                Enumerable.Repeat<SlotItem?>(null, 10).ToList(),
                Enumerable.Repeat<SlotItem?>(null, 10).ToList(),
                true
            );
        }
    }
}