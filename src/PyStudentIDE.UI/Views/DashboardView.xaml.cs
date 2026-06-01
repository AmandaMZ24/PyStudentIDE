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
                var cursos = await App.Api.GetCoursesAsync(App.CurrentUserId);
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

                AssignmentsList.ItemsSource = pendientes;

                if (pendientes.Any())
                    CountdownText.Text = $"Tienes {pendientes.Count} asignaciones pendientes";
                else
                    CountdownText.Text = "No tienes asignaciones pendientes";
            }
            catch { }
        };
    }
}
