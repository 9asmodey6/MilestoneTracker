namespace MilestoneTracker.Domain.Entities.ValueObjects;

public class AgeInfo
{
    private int Years { get; }
    private int Months { get; }
    private int Days { get; }
    public int TotalDays { get; }
    
    private AgeInfo(int years, int months, int days, int totalDays)
    {
        Years = years;
        Months = months;
        Days = days;
        TotalDays = totalDays;
    }
    
    public static AgeInfo Calculate(DateTime birthDate, DateTime eventDate)
    {
        if (eventDate < birthDate)
            throw new ArgumentException("Event date cannot be before birth date");
            
        var totalDays = (eventDate - birthDate).Days;
        
        int years = 0, months = 0, days = 0;
        var temp = birthDate;
        
        while (temp.AddYears(1) <= eventDate)
        {
            temp = temp.AddYears(1);
            years++;
        }
        
        while (temp.AddMonths(1) <= eventDate)
        {
            temp = temp.AddMonths(1);
            months++;
        }
        
        days = (eventDate - temp).Days;
        
        return new AgeInfo(years, months, days, totalDays);
    }
    
    public override string ToString()
    {
        if (Years == 0 && Months == 0)
            return $"{Days} дн.";
        if (Years == 0)
            return $"{Months} мес. {Days} дн.";
        return $"{Years} г. {Months} мес.";
    }
    
    public string ToDetailedString()
    {
        return $"{Years} лет, {Months} месяцев, {Days} дней";
    }
}