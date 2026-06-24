using System.Windows.Controls;
using PyStudentIDE.Application.DTOs;

namespace PyStudentIDE.UI.Views;

public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            try
            {
                var isTeacher = App.CurrentUserRole == "DOCENTE";
                TeacherSection.Visibility = isTeacher ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                StudentSection.Visibility = isTeacher ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;

                var cursos = await App.Api.GetCoursesAsync(App.CurrentUserId);
                CourseCount.Text = cursos.Count.ToString();

                if (isTeacher)
                {
                    Stat2Label.Text = "Asignaciones";
                    var summary = new List<CourseSummaryDTO>();
                    int totalAssignments = 0;

                    foreach (var curso in cursos)
                    {
                        var tareas = await App.Api.GetAssignmentsByCourseAsync(curso.IdCurso);
                        var estudiantes = await App.Api.GetStudentsByCourseAsync(curso.IdCurso);
                        int totalEntregas = 0;
                        foreach (var t in tareas)
                        {
                            var entregas = await App.Api.GetDeliveriesByAssignmentAllAsync(t.IdAsignacion);
                            totalEntregas += entregas.Count;
                        }
                        totalAssignments += tareas.Count;
                        summary.Add(new CourseSummaryDTO
                        {
                            NombreCurso = $"{curso.Codigo} - {curso.Nombre} ({curso.Periodo})",
                            TotalAsignaciones = tareas.Count,
                            TotalEntregas = totalEntregas,
                            TotalEstudiantes = estudiantes.Count
                        });
                    }

                    PendingCount.Text = totalAssignments.ToString();
                    StatusText.Text = $"{cursos.Count} cursos activos";
                    DeliverySummaryList.ItemsSource = summary;
                }
                else
                {
                    Stat2Label.Text = "Pendientes";
                    CoursesList.ItemsSource = cursos.Select(c => $"{c.Codigo} - {c.Nombre} ({c.Periodo})").ToList();

                    var allAssignments = new List<AssignmentDTO>();
                    foreach (var curso in cursos)
                    {
                        var tareas = await App.Api.GetAssignmentsByCourseAsync(curso.IdCurso);
                        allAssignments.AddRange(tareas);
                    }

                    var pendientes = allAssignments
                        .Where(a => a.FechaLimite > DateTime.UtcNow)
                        .Select(a => $"{a.Titulo} (vence: {a.FechaLimite:g})")
                        .ToList();

                    PendingCount.Text = pendientes.Count.ToString();
                    AssignmentsList.ItemsSource = pendientes;

                    if (pendientes.Any())
                    {
                        CountdownText.Text = $"Tienes {pendientes.Count} asignaciones pendientes";
                        StatusText.Text = "Activo";
                    }
                    else
                    {
                        CountdownText.Text = "No tienes asignaciones pendientes";
                        StatusText.Text = "Al día";
                    }
                }
            }
            catch { }
        };
    }
}

public class CourseSummaryDTO
{
    public string NombreCurso { get; set; } = string.Empty;
    public int TotalAsignaciones { get; set; }
    public int TotalEntregas { get; set; }
    public int TotalEstudiantes { get; set; }
}
