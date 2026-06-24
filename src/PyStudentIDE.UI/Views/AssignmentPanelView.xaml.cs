using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using PyStudentIDE.Application.DTOs;

namespace PyStudentIDE.UI.Views;

public partial class AssignmentPanelView : UserControl
{
    private List<AssignmentViewItem> _assignmentViews = new();
    private Dictionary<int, string> _courseNames = new();
    private readonly Dictionary<int, List<DeliveryResponse>> _deliveries = new();
    private AssignmentViewItem? _selectedAssignment;
    private int _lastEntregaId;
    private string? _selectedFilePath;

    public AssignmentPanelView()
    {
        InitializeComponent();
        Loaded += async (_, _) => await LoadAssignments();
    }

    private async Task LoadAssignments()
    {
        try
        {
            var cursos = await App.Api.GetCoursesAsync(App.CurrentUserId);
            _courseNames = cursos.ToDictionary(c => c.IdCurso, c => $"{c.Codigo} - {c.Nombre}");
            _assignmentViews.Clear();

            foreach (var curso in cursos)
            {
                var tareas = await App.Api.GetAssignmentsByCourseAsync(curso.IdCurso);
                foreach (var t in tareas)
                {
                    _assignmentViews.Add(new AssignmentViewItem
                    {
                        Dto = t,
                        CursoInfo = _courseNames.GetValueOrDefault(t.IdCurso, $"Curso #{t.IdCurso}")
                    });
                }
            }

            var entregas = await App.Api.GetDeliveriesByStudentAsync(App.CurrentUserId);
            _deliveries.Clear();
            foreach (var e in entregas)
            {
                if (!_deliveries.ContainsKey(e.IdAsignacion))
                    _deliveries[e.IdAsignacion] = new();
                _deliveries[e.IdAsignacion].Add(e);
            }

            var now = DateTime.UtcNow;
            PendingList.ItemsSource = _assignmentViews
                .Where(a => a.Dto.FechaLimite > now && (!_deliveries.ContainsKey(a.Dto.IdAsignacion) || !_deliveries[a.Dto.IdAsignacion].Any()))
                .ToList();
            DeliveredList.ItemsSource = _assignmentViews
                .Where(a => _deliveries.ContainsKey(a.Dto.IdAsignacion) && _deliveries[a.Dto.IdAsignacion].Any())
                .ToList();
            OverdueList.ItemsSource = _assignmentViews
                .Where(a => a.Dto.FechaLimite < now && (!_deliveries.ContainsKey(a.Dto.IdAsignacion) || !_deliveries[a.Dto.IdAsignacion].Any()))
                .ToList();

            if (_assignmentViews.Any())
                ShowAssignment(_assignmentViews.First());
        }
        catch { }
    }

    private void OnPendingSelection(object sender, SelectionChangedEventArgs e)
    {
        if (PendingList.SelectedItem is AssignmentViewItem a) ShowAssignment(a);
    }

    private void OnDeliveredSelection(object sender, SelectionChangedEventArgs e)
    {
        if (DeliveredList.SelectedItem is AssignmentViewItem a) ShowAssignment(a);
    }

    private void OnOverdueSelection(object sender, SelectionChangedEventArgs e)
    {
        if (OverdueList.SelectedItem is AssignmentViewItem a) ShowAssignment(a);
    }

    private async void ShowAssignment(AssignmentViewItem a)
    {
        _selectedAssignment = a;
        AssignmentTitle.Text = a.Titulo;
        AssignmentDescription.Text = a.Dto.Descripcion ?? "Sin descripción";
        DeadlineText.Text = $"{a.CursoInfo} | Fecha límite: {a.Dto.FechaLimite:g}";
        FeedbackPanel.Visibility = Visibility.Collapsed;

        var tieneEntrega = _deliveries.TryGetValue(a.Dto.IdAsignacion, out var list) && list.Any();
        SubmitButton.Visibility = tieneEntrega ? Visibility.Collapsed : Visibility.Visible;
        ResubmitButton.Visibility = tieneEntrega ? Visibility.Visible : Visibility.Collapsed;
        TestButton.Visibility = tieneEntrega ? Visibility.Visible : Visibility.Collapsed;
        TestResultsPanel.Visibility = Visibility.Collapsed;

        if (tieneEntrega && list!.Any())
        {
            var ultima = list!.OrderByDescending(d => d.NumeroIntento).First();
            _lastEntregaId = ultima.IdEntrega;
            DeadlineText.Text += $" | Último intento: #{ultima.NumeroIntento} ({ultima.FechaEntrega:g})";
            if (ultima.Calificacion.HasValue)
                DeadlineText.Text += $" | Calificación: {ultima.Calificacion}";

            try
            {
                var retro = await App.Api.GetRetroalimentacionByEntregaAsync(ultima.IdEntrega);
                if (retro.Any())
                {
                    FeedbackList.ItemsSource = retro;
                    FeedbackPanel.Visibility = Visibility.Visible;
                }
            }
            catch { }
        }
    }

    private async void OnSubmit(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = "Python Files|*.py" };
        if (dialog.ShowDialog() != true) return;

        _selectedFilePath = dialog.FileName;
        var content = await File.ReadAllBytesAsync(_selectedFilePath);

        if (_selectedAssignment == null) return;

        var dto = new DeliveryDTO
        {
            IdAsignacion = _selectedAssignment.Dto.IdAsignacion,
            IdEstudiante = App.CurrentUserId,
            RutaArchivo = _selectedFilePath,
            ContenidoBase64 = Convert.ToBase64String(content)
        };

        DeliveryResultDTO? result;
        if (_lastEntregaId > 0)
            result = await App.Api.ResubmitDeliveryAsync(dto, _lastEntregaId);
        else
            result = await App.Api.SubmitDeliveryAsync(dto);

        if (result != null && result.Exitoso)
        {
            _lastEntregaId = result.IdEntrega;
            MessageBox.Show($"Entrega exitosa (ID: {result.IdEntrega})\nFirma: {result.FirmaDigital?[..20]}...\nHora: {result.Timestamp:g}",
                "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            ResubmitButton.Visibility = Visibility.Visible;
            SubmitButton.Visibility = Visibility.Collapsed;
            await LoadAssignments();
        }
        else
        {
            var msg = result?.Mensaje ?? "Error al entregar";
            MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OnResubmit(object sender, RoutedEventArgs e)
    {
        OnSubmit(sender, e);
    }

    private async void OnJoinCourse(object sender, RoutedEventArgs e)
    {
        var cursoIdText = JoinCourseIdBox.Text.Trim();
        if (!int.TryParse(cursoIdText, out var cursoId))
        {
            JoinResult.Text = "Ingresa un ID de curso válido";
            JoinResult.Foreground = System.Windows.Media.Brushes.Red;
            return;
        }

        try
        {
            var request = new JoinCursoRequest
            {
                IdUsuario = App.CurrentUserId,
                TipoParticipacion = "ESTUDIANTE"
            };

            var ok = await App.Api.JoinCourseAsync(cursoId, request);
            if (ok)
            {
                JoinResult.Text = "✅ Unido exitosamente";
                JoinResult.Foreground = System.Windows.Media.Brushes.Green;
                JoinCourseIdBox.Clear();
                await LoadAssignments();
            }
            else
            {
                JoinResult.Text = "❌ Error al unirse";
                JoinResult.Foreground = System.Windows.Media.Brushes.Red;
            }
        }
        catch (Exception ex)
        {
            JoinResult.Text = $"❌ {ex.Message}";
            JoinResult.Foreground = System.Windows.Media.Brushes.Red;
        }
    }

    private async void OnRunTests(object sender, RoutedEventArgs e)
    {
        if (_selectedAssignment == null || _lastEntregaId <= 0)
        {
            MessageBox.Show("Selecciona una asignación entregada primero", "Pruebas", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrEmpty(_selectedFilePath))
        {
            var dialog = new OpenFileDialog { Filter = "Python Files|*.py" };
            if (dialog.ShowDialog() != true) return;
            _selectedFilePath = dialog.FileName;
        }

        try
        {
            TestButton.IsEnabled = false;
            TestButton.Content = "⏳ Ejecutando...";

            var result = await App.Api.ExecuteTestsAsync(
                _selectedAssignment.Dto.IdAsignacion,
                _lastEntregaId,
                _selectedFilePath);

            if (result != null)
            {
                TestSummary.Text = $"Total: {result.Total} | ✅ Aprobadas: {result.Aprobadas} | ❌ Fallidas: {result.Fallidas}";
                TestResultsList.ItemsSource = result.Resultados;
                TestResultsPanel.Visibility = Visibility.Visible;
            }
            else
            {
                MessageBox.Show("Error al ejecutar las pruebas", "Pruebas", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Pruebas", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            TestButton.IsEnabled = true;
            TestButton.Content = "🧪 Ejecutar Pruebas Automáticas";
        }
    }
}

public class AssignmentViewItem
{
    public AssignmentDTO Dto { get; set; } = new();
    public string CursoInfo { get; set; } = string.Empty;
    public string Titulo => Dto.Titulo;
}
