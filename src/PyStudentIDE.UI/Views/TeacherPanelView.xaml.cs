using System.Windows;
using System.Windows.Controls;
using PyStudentIDE.Application.DTOs;

namespace PyStudentIDE.UI.Views;

public partial class TeacherPanelView : UserControl
{
    private List<CursoResponse> _cursos = new();
    private CursoResponse? _selectedCourse;
    private Dictionary<int, string> _studentNames = new();
    private DeliveryWithStudentDTO? _selectedDelivery;

    public TeacherPanelView()
    {
        InitializeComponent();
        TeacherName.Text = $"Bienvenido, {App.CurrentUserName}";
        Loaded += async (_, _) => await LoadCourses();
    }

    private async System.Threading.Tasks.Task LoadCourses()
    {
        try
        {
            _cursos = (await App.Api.GetCoursesAsync(App.CurrentUserId)).ToList();
            CoursesList.ItemsSource = _cursos;

            if (_cursos.Any())
            {
                CoursesList.SelectedIndex = 0;
                await ShowCourse(_cursos[0]);
            }
        }
        catch { }
    }

    private async void OnCourseSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is CursoResponse c)
            await ShowCourse(c);
    }

    private async System.Threading.Tasks.Task ShowCourse(CursoResponse curso)
    {
        _selectedCourse = curso;
        CourseTitle.Text = $"{curso.Codigo} - {curso.Nombre}";
        CourseInfo.Text = $"Período: {curso.Periodo} | Participación: {curso.TipoParticipacion}";

        try
        {
            var tareas = await App.Api.GetAssignmentsByCourseAsync(curso.IdCurso);
            AssignmentsList.ItemsSource = tareas;

            var estudiantes = await App.Api.GetStudentsByCourseAsync(curso.IdCurso);
            StudentsList.ItemsSource = estudiantes;
            StudentCount.Text = $"Estudiantes matriculados: {estudiantes.Count}";
            _studentNames = estudiantes.ToDictionary(e => e.IdUsuario, e => e.Nombre);

            await LoadKeyInfo(curso.IdCurso);
        }
        catch { }
    }

    private async System.Threading.Tasks.Task LoadKeyInfo(int cursoId)
    {
        try
        {
            var llave = await App.Api.ObtenerLlaveActivaAsync(cursoId);
            if (llave != null)
            {
                ActiveKeyInfo.ItemsSource = new List<LlaveCursoResponse> { llave };
                KeyStatus.Text = "✅ Llave activa encontrada";
            }
            else
            {
                ActiveKeyInfo.ItemsSource = null;
                KeyStatus.Text = "⚠️ No hay una llave activa para este curso. Presiona 'Generar Llave' para crear una.";
            }
        }
        catch
        {
            KeyStatus.Text = "Error al consultar llaves";
        }
    }

    private async void OnCreateCourse(object sender, RoutedEventArgs e)
    {
        var codigo = CourseCodeBox.Text.Trim();
        var nombre = CourseNameBox.Text.Trim();
        var periodo = CoursePeriodBox.Text.Trim();

        if (string.IsNullOrEmpty(codigo) || string.IsNullOrEmpty(nombre))
        {
            CourseError.Text = "Código y nombre son obligatorios";
            CourseError.Visibility = Visibility.Visible;
            return;
        }

        CourseError.Visibility = Visibility.Collapsed;

        try
        {
            var request = new CreateCursoRequest
            {
                Codigo = codigo,
                Nombre = nombre,
                Periodo = periodo
            };

            var result = await App.Api.CreateCourseAsync(request, App.CurrentUserId);
            if (result != null)
            {
                CourseCodeBox.Clear();
                CourseNameBox.Clear();
                CoursePeriodBox.Text = "2026-I";
                await LoadCourses();
            }
            else
            {
                CourseError.Text = "Error al crear el curso";
                CourseError.Visibility = Visibility.Visible;
            }
        }
        catch (Exception ex)
        {
            CourseError.Text = $"Error: {ex.Message}";
            CourseError.Visibility = Visibility.Visible;
        }
    }

    private async void OnCreateAssignment(object sender, RoutedEventArgs e)
    {
        if (CoursesList.SelectedItem is not CursoResponse curso)
        {
            AssignmentError.Text = "Selecciona un curso primero";
            AssignmentError.Visibility = Visibility.Visible;
            return;
        }

        var titulo = NewTitleBox.Text.Trim();
        var desc = NewDescBox.Text.Trim();

        if (string.IsNullOrEmpty(titulo))
        {
            AssignmentError.Text = "El título es obligatorio";
            AssignmentError.Visibility = Visibility.Visible;
            return;
        }

        AssignmentError.Visibility = Visibility.Collapsed;

        try
        {
            var fechaLimite = NewDeadlinePicker.SelectedDate.HasValue
                ? NewDeadlinePicker.SelectedDate.Value.ToUniversalTime()
                : DateTime.UtcNow.AddDays(30);

            DateTime? inicioPrueba = NewTestStartPicker.SelectedDate.HasValue
                ? NewTestStartPicker.SelectedDate.Value.ToUniversalTime()
                : null;
            DateTime? finPrueba = NewTestEndPicker.SelectedDate.HasValue
                ? NewTestEndPicker.SelectedDate.Value.ToUniversalTime()
                : null;

            var dto = new AssignmentDTO
            {
                IdCurso = curso.IdCurso,
                Titulo = titulo,
                Descripcion = desc,
                FechaLimite = fechaLimite,
                InicioPeriodoPrueba = inicioPrueba,
                FinPeriodoPrueba = finPrueba
            };

            var id = await App.Api.CreateAssignmentAsync(dto, App.CurrentUserId);
            if (id.HasValue)
            {
                NewTitleBox.Clear();
                NewDescBox.Clear();
                NewDeadlinePicker.SelectedDate = null;
                NewTestStartPicker.SelectedDate = null;
                NewTestEndPicker.SelectedDate = null;
                await ShowCourse(curso);
            }
            else
            {
                AssignmentError.Text = "Error al crear la asignación";
                AssignmentError.Visibility = Visibility.Visible;
            }
        }
        catch (Exception ex)
        {
            AssignmentError.Text = $"Error: {ex.Message}";
            AssignmentError.Visibility = Visibility.Visible;
        }
    }

    private async void OnAssignmentSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is AssignmentDTO a)
        {
            try
            {
                var entregas = await App.Api.GetDeliveriesByAssignmentAllAsync(a.IdAsignacion);

                var entregasConNombre = entregas.Select(d => new DeliveryWithStudentDTO
                {
                    IdEntrega = d.IdEntrega,
                    IdAsignacion = d.IdAsignacion,
                    IdEstudiante = d.IdEstudiante,
                    EstudianteNombre = _studentNames.TryGetValue(d.IdEstudiante, out var name) ? name : $"Estudiante #{d.IdEstudiante}",
                    FechaEntrega = d.FechaEntrega,
                    Estado = d.Estado,
                    Calificacion = d.Calificacion,
                    EsTardia = d.EsTardia,
                    NumeroIntento = d.NumeroIntento,
                    FirmaDigital = d.FirmaDigital
                }).ToList();

                DeliveriesList.ItemsSource = entregasConNombre;
                DeliveryInfo.Text = $"Entregas para: {a.Titulo} ({entregas.Count})";
                DeliveryDetail.Text = string.Empty;
                FeedbackList.ItemsSource = null;
                FeedbackCommentBox.Clear();
                FeedbackGradeBox.Clear();
            }
            catch { }
        }
    }

    private async void OnDeliverySelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is DeliveryWithStudentDTO d)
        {
            _selectedDelivery = d;
            DeliveryDetail.Text = $"Entrega #{d.NumeroIntento} | " +
                $"Estudiante: {d.EstudianteNombre} | " +
                $"Fecha: {d.FechaEntrega:g} | " +
                $"Estado: {d.Estado} | " +
                (d.Calificacion.HasValue ? $"Calificación: {d.Calificacion}" : "Sin calificar") +
                (d.EsTardia ? " | ⚠️ Tardía" : "");
            ViewFileButton.Visibility = Visibility.Visible;

            try
            {
                var retro = await App.Api.GetRetroalimentacionByEntregaAsync(d.IdEntrega);
                FeedbackList.ItemsSource = retro;
            }
            catch { }
        }
    }

    private async void OnSendFeedback(object sender, RoutedEventArgs e)
    {
        if (DeliveriesList.SelectedItem is not DeliveryWithStudentDTO delivery)
        {
            FeedbackError.Text = "Selecciona una entrega primero";
            FeedbackError.Visibility = Visibility.Visible;
            return;
        }

        var comentario = FeedbackCommentBox.Text.Trim();
        if (string.IsNullOrEmpty(comentario))
        {
            FeedbackError.Text = "El comentario es obligatorio";
            FeedbackError.Visibility = Visibility.Visible;
            return;
        }

        FeedbackError.Visibility = Visibility.Collapsed;

        decimal? calificacion = null;
        if (!string.IsNullOrEmpty(FeedbackGradeBox.Text.Trim()))
        {
            if (decimal.TryParse(FeedbackGradeBox.Text.Trim(), out var grade))
                calificacion = grade;
            else
            {
                FeedbackError.Text = "Calificación inválida (ingresa un número)";
                FeedbackError.Visibility = Visibility.Visible;
                return;
            }
        }

        try
        {
            var dto = new CreateRetroalimentacionDTO
            {
                IdEntrega = delivery.IdEntrega,
                Comentario = comentario,
                Calificacion = calificacion
            };

            var result = await App.Api.CreateRetroalimentacionAsync(dto);
            if (result != null)
            {
                FeedbackCommentBox.Clear();
                FeedbackGradeBox.Clear();
                var retro = await App.Api.GetRetroalimentacionByEntregaAsync(delivery.IdEntrega);
                FeedbackList.ItemsSource = retro;
                FeedbackError.Text = "Retroalimentación enviada";
                FeedbackError.Visibility = Visibility.Visible;
            }
            else
            {
                FeedbackError.Text = "Error al enviar retroalimentación";
                FeedbackError.Visibility = Visibility.Visible;
            }
        }
        catch (Exception ex)
        {
            FeedbackError.Text = $"Error: {ex.Message}";
            FeedbackError.Visibility = Visibility.Visible;
        }
    }

    private async void OnGenerateKey(object sender, RoutedEventArgs e)
    {
        if (_selectedCourse == null) return;

        try
        {
            var result = await App.Api.GenerarLlaveCursoAsync(_selectedCourse.IdCurso);
            if (result != null)
            {
                KeyStatus.Text = $"✅ Llave generada: {result.Algoritmo}\n" +
                    $"ID: {result.IdLlave}\n" +
                    $"Pública: {result.LlavePublicaXml[..Math.Min(60, result.LlavePublicaXml.Length)]}...\n" +
                    $"⚠️ Guarda la llave privada en un lugar seguro (solo se muestra una vez).";

                ActiveKeyInfo.ItemsSource = new List<LlaveCursoResponse>
                {
                    new LlaveCursoResponse
                    {
                        IdLlave = result.IdLlave,
                        IdCurso = _selectedCourse.IdCurso,
                        Algoritmo = result.Algoritmo,
                        Activa = true,
                        FechaCreacion = DateTime.UtcNow
                    }
                };
            }
            else
            {
                KeyStatus.Text = "Error al generar la llave";
            }
        }
        catch (Exception ex)
        {
            KeyStatus.Text = $"Error: {ex.Message}";
        }
    }

    private async void OnDeactivateKey(object sender, RoutedEventArgs e)
    {
        if (_selectedCourse == null) return;

        try
        {
            var llave = await App.Api.ObtenerLlaveActivaAsync(_selectedCourse.IdCurso);
            if (llave == null)
            {
                KeyStatus.Text = "No hay una llave activa para desactivar";
                return;
            }

            var ok = await App.Api.DesactivarLlaveAsync(llave.IdLlave);
            if (ok)
            {
                KeyStatus.Text = "✅ Llave desactivada exitosamente";
                ActiveKeyInfo.ItemsSource = null;
            }
            else
            {
                KeyStatus.Text = "Error al desactivar la llave";
            }
        }
        catch (Exception ex)
        {
            KeyStatus.Text = $"Error: {ex.Message}";
        }
    }

    private async void OnViewFile(object sender, RoutedEventArgs e)
    {
        if (_selectedDelivery == null) return;
        try
        {
            var contenido = await App.Api.GetDeliveryFileContentAsync(_selectedDelivery.IdEntrega);
            if (contenido == null)
            {
                MessageBox.Show("No se encontró el archivo de esta entrega.", "Archivo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var window = new Window
            {
                Title = $"Entrega #{_selectedDelivery.NumeroIntento} - {_selectedDelivery.EstudianteNombre}",
                Width = 700,
                Height = 500,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this),
                Content = new TextBox
                {
                    Text = contenido,
                    IsReadOnly = true,
                    FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                    FontSize = 13,
                    TextWrapping = TextWrapping.Wrap,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                    Margin = new Thickness(10)
                }
            };
            window.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al obtener el archivo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

public class DeliveryWithStudentDTO
{
    public int IdEntrega { get; set; }
    public int IdAsignacion { get; set; }
    public int IdEstudiante { get; set; }
    public string EstudianteNombre { get; set; } = string.Empty;
    public DateTime FechaEntrega { get; set; }
    public string Estado { get; set; } = string.Empty;
    public decimal? Calificacion { get; set; }
    public bool EsTardia { get; set; }
    public int NumeroIntento { get; set; }
    public string? FirmaDigital { get; set; }
}
