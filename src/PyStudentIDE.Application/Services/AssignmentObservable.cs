using PyStudentIDE.Application.Interfaces;

namespace PyStudentIDE.Application.Services;

public class AssignmentObservable : IAssignmentObservable
{
    private readonly List<IAssignmentObserver> _observers = new();

    public void Subscribe(IAssignmentObserver observer)
    {
        if (!_observers.Contains(observer))
            _observers.Add(observer);
    }

    public void Unsubscribe(IAssignmentObserver observer)
    {
        _observers.Remove(observer);
    }

    public void NotifyAssignmentPublished(int assignmentId)
    {
        foreach (var observer in _observers)
            observer.OnAssignmentPublished(assignmentId);
    }

    public void NotifyDeliveryReceived(int deliveryId)
    {
        foreach (var observer in _observers)
            observer.OnDeliveryReceived(deliveryId);
    }

    public void NotifyIntegrityViolation(int studentId, string fileName)
    {
        foreach (var observer in _observers)
            observer.OnIntegrityViolation(studentId, fileName);
    }
}
