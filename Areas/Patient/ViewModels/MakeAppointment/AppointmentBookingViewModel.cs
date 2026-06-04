using System;
using System.Collections.Generic;

namespace Wellora.Areas.Patient.ViewModels.MakeAppointment
{
    public class AppointmentBookingViewModel
    {
        // =========================
        // Doctor Info
        // =========================
        public int DoctorId { get; set; }
        public string? DoctorName { get; set; }
        public string? Specialization { get; set; }
        public string? SubSpecialization { get; set; }
        public string? ProfilePhoto { get; set; }
        public decimal ConsultationFee { get; set; }

        // =========================
        // Calendar
        // =========================
        public List<DateTime> AvailableDates { get; set; } = new();
        public DateTime? SelectedDate { get; set; }

        public List<CalendarCell> CalendarCells { get; set; } = new();

        // =========================
        // Slots (FIXED GRID SYSTEM)
        // =========================
        public List<SlotItem?> MorningSlots { get; set; } = new();
        public List<SlotItem?> AfternoonSlots { get; set; } = new();
        public List<SlotItem?> EveningSlots { get; set; } = new();

        public string? SelectedSlot { get; set; }

        // =========================
        // Payment
        // =========================
        public string? PaymentMethod { get; set; } = "Onsite";

        public string? Notes { get; set; }

        // =========================
        // Flags
        // =========================
        public bool NoSlotsAvailable { get; set; } = false;
    }

    // =========================
    // Calendar Cell Model
    // =========================
    public class CalendarCell
    {
        public DateTime? Date { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsSelected { get; set; }
    }
}