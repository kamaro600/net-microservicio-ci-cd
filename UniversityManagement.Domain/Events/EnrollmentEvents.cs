namespace UniversityManagement.Domain.Events;

public abstract class DomainEvent
{
    public DateTime OccurredOn { get; protected set; }
    public Guid EventId { get; protected set; }

    protected DomainEvent()
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
    }
}

public class StudentEnrolledEvent : DomainEvent
{
    public int StudentId { get; }
    public int CareerId { get; }
    public string StudentName { get; }
    public string CareerName { get; }
    public string StudentEmail { get; }
    public DateTime EnrollmentDate { get; }
    public string EnrolledBy { get; }

    public StudentEnrolledEvent(int studentId, int careerId, string studentName, string careerName, 
        string studentEmail, DateTime enrollmentDate, string enrolledBy)
    {
        StudentId = studentId;
        CareerId = careerId;
        StudentName = studentName;
        CareerName = careerName;
        StudentEmail = studentEmail;
        EnrollmentDate = enrollmentDate;
        EnrolledBy = enrolledBy;
    }
}

public class StudentUnenrolledEvent : DomainEvent
{
    public int StudentId { get; }
    public int CareerId { get; }
    public string StudentName { get; }
    public string CareerName { get; }
    public string StudentEmail { get; }
    public DateTime UnenrollmentDate { get; }
    public string UnenrolledBy { get; }

    public StudentUnenrolledEvent(int studentId, int careerId, string studentName, string careerName,
        string studentEmail, DateTime unenrollmentDate, string unenrolledBy)
    {
        StudentId = studentId;
        CareerId = careerId;
        StudentName = studentName;
        CareerName = careerName;
        StudentEmail = studentEmail;
        UnenrollmentDate = unenrollmentDate;
        UnenrolledBy = unenrolledBy;
    }
}