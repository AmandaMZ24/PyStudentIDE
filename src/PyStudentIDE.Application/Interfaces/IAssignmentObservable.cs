namespace PyStudentIDE.Application.Interfaces;

public interface IAssignmentObservable
{
    void Subscribe(IAssignmentObserver observer);
    void Unsubscribe(IAssignmentObserver observer);
    void NotifyAssignmentPublished(int assignmentId);
    void NotifyDeliveryReceived(int deliveryId);
    void NotifyIntegrityViolation(int studentId, string fileName);
}
