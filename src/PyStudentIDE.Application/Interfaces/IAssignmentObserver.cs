using PyStudentIDE.Application.DTOs;

namespace PyStudentIDE.Application.Interfaces;

public interface IAssignmentObserver
{
    void OnAssignmentPublished(int assignmentId);
    void OnDeliveryReceived(int deliveryId);
    void OnIntegrityViolation(int studentId, string fileName);
}
