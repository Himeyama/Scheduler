using System;
using Microsoft.UI.Xaml.Media;
using System.Linq;
using Common;
using System.IO;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.Networking.NetworkOperators;
using Windows.System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;


namespace Scheduler;

public class ProjectPlanningSaveFormat
{
    public List<string> Assignees { get; set; } = new();
    public List<Task> Tasks { get; set; } = new();
}

public class Task
{
    public string Name { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime StartPlanDate { get; set; }
    public DateTime EndPlanDate { get; set; }
    public string StartDateText { get; set; }
    public string EndDateText { get; set; }
    public string StartPlanDateText { get; set; }
    public string EndPlanDateText { get; set; }
    public string Priority { get; set; }
    public string Assignee { get; set; }
    public string Status { get; set; }
    public string Description { get; set; }
}

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        Title = AppTitleTextBlock.Text;

        Status.statusBar = StatusBar;
        Status.dispatcherQueue = DispatcherQueue;
        // ZoomIn.KeyboardAcceleratorTextOverride = ZoomInText.Text;
        // ZoomOut.KeyboardAcceleratorTextOverride = ZoomOutText.Text;

        ClearInput();
        Load();
    }

    void AutoSave_Toggled(object sender, RoutedEventArgs e)
    {
        try
        {
            // EnableAutoSave.Visibility = AutoSave.IsOn ? Visibility.Visible : Visibility.Collapsed;
            // DisableAutoSave.Visibility = AutoSave.IsOn ? Visibility.Collapsed : Visibility.Visible;
        }
        catch (Exception ex)
        {
            Status.AddMessage(ex.Message);
        }
    }

    async void ClickOpen(object sender, RoutedEventArgs e)
    {
        await FilePicker.Open(this);
    }

    async void ClickSave(object sender, RoutedEventArgs e)
    {
        await FilePicker.Save(this, "Save");
    }

    async void ClickSaveAs(object sender, RoutedEventArgs e)
    {
        await FilePicker.Save(this, "Save as");
    }

    void ClickZoomIn(object sender, RoutedEventArgs e)
    {
        Status.AddMessage($"Zoom In");
    }

    void ClickZoomOut(object sender, RoutedEventArgs e)
    {
        Status.AddMessage($"Zoom out");
    }

    void ClickRestoreDefaultZoom(object sender, RoutedEventArgs e)
    {
        Status.AddMessage($"Restore default zoom");
    }

    async void ClickAbout(object sender, RoutedEventArgs e)
    {
        await Dialog.Show(Content, "This app is an example app for Windows App SDK!", "About");
        Status.AddMessage($"Thank you for using this app!");
    }

    void ClickExit(object sender, RoutedEventArgs e)
    {
        Close();
    }

    // 追加: 登録ボタンのクリックイベント
    void RegisterButton_Click(object sender, RoutedEventArgs e)
    {
        // 入力値を取得
        string taskName = InputTaskTextBox.Text;
        if (taskName == string.Empty)
        {
            _ = Dialog.ShowError(Content, "タスク名が入力されていません");
            return;
        }

        try
        {
            string priority = "";
            if (InputPriorityComboBox.SelectedItem is ComboBoxItem item)
            {
                priority = $"{item.Content}";
            }
            else if (InputPriorityComboBox.SelectedItem is string textItem)
            {
                priority = textItem;
            }

            string assignee = "";
            if (InputPersonInChargeComboBox.SelectedItem is ComboBoxItem assigneeItem)
            {
                assignee = $"{assigneeItem.Content}";
            }
            else if (InputPersonInChargeComboBox.SelectedItem is string assigneeText)
            {
                assignee = assigneeText;
            }

            string status = "";
            if (InputStatusComboBox.SelectedItem is string statusText)
            {
                status = statusText;
            }
            else if (InputStatusComboBox.SelectedItem is ComboBoxItem statusItem)
            {
                status = $"{statusItem.Content}";
            }

            Task task = new()
            {
                Name = taskName,
                StartDate = InputStartDatePicker.SelectedDate?.DateTime,
                EndDate = InputEndDatePicker.SelectedDate?.DateTime,
                StartPlanDate = InputStartPlanDatePicker.SelectedDate?.DateTime ?? DateTime.Now,
                EndPlanDate = InputEndPlanDatePicker.SelectedDate?.DateTime ?? DateTime.Now,
                Priority = priority,
                Assignee = assignee,
                Status = status,
                Description = DescriptionInputTextBox.Text // 必要に応じて入力欄から取得する
            };

            if (task.EndPlanDate < task.StartPlanDate)
            {
                _ = Dialog.ShowError(Content, "終了予定日が開始予定日より前になっています。");
                return;
            }

            if (task.EndDate != null && task.StartDate != null && task.EndDate < task.StartDate)
            {
                _ = Dialog.ShowError(Content, "終了日が開始日より前になっています。");
                return;
            }

            task.StartPlanDateText = task.StartPlanDate.ToString("yyyy/MM/dd");
            task.EndPlanDateText = task.EndPlanDate.ToString("yyyy/MM/dd");
            task.StartDateText = task.StartDate?.ToString("yyyy/MM/dd");
            task.EndDateText = task.EndDate?.ToString("yyyy/MM/dd");

            bool alreadyTask = false;
            // 既存のタスク名をチェック

            List<Task> tasks = [];
            foreach (Task existingTask in TaskList.Items)
            {
                if (existingTask.Name == taskName)
                {
                    tasks.Add(task);
                    alreadyTask = true;
                }
                else
                {
                    tasks.Add(existingTask);
                }
            }
            TaskList.Items.Clear();
            foreach (Task taskItem in tasks)
            {
                TaskList.Items.Add(taskItem);
            }

            if (!alreadyTask)
            {
                TaskList.Items.Add(task);
            }

            ClearInput();

            Save();
        }
        catch (Exception ex)
        {
            File.AppendAllText("debug.txt", ex.ToString() + Environment.NewLine);
        }
    }

    void ClearInputButton_Click(object sender, RoutedEventArgs e)
    {
        ClearInput();
    }

    void ClearInput()
    {
        // 入力欄をクリア
        InputTaskTextBox.Text = "";
        InputStartDatePicker.SelectedDate = null;
        InputEndDatePicker.SelectedDate = null;
        InputStartPlanDatePicker.SelectedDate = DateTime.Now;
        InputEndPlanDatePicker.SelectedDate = DateTime.Now;
        DescriptionInputTextBox.Text = "";
        InputPersonInChargeComboBox.SelectedIndex = 0;
        InputPriorityComboBox.SelectedIndex = 1;
        InputStatusComboBox.SelectedIndex = 0;
    }

    void InputTaskTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        AddButton.IsEnabled = !string.IsNullOrWhiteSpace(InputTaskTextBox.Text);
    }

    void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (TaskList.SelectedItem is Task selectedTask)
        {
            TaskList.Items.Remove(selectedTask);
            Save();
        }
    }

    void TaskList_Tapped(object sender, TappedRoutedEventArgs e)
    {
        TaskList_SelectionChanged(sender, null);
    }

    void TaskList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (TaskList.SelectedItem is Task selectedTask)
        {
            InputTaskTextBox.Text = selectedTask.Name;
            InputStartDatePicker.SelectedDate = selectedTask.StartDate;
            InputEndDatePicker.SelectedDate = selectedTask.EndDate;
            InputStartPlanDatePicker.SelectedDate = selectedTask.StartPlanDate;
            InputEndPlanDatePicker.SelectedDate = selectedTask.EndPlanDate;
            DescriptionInputTextBox.Text = selectedTask.Description;
            InputPriorityComboBox.SelectedItem = FindComboBoxItemByContent(InputPriorityComboBox, selectedTask.Priority);
            InputStatusComboBox.SelectedItem = FindComboBoxItemByContent(InputStatusComboBox, selectedTask.Status);
            InputPersonInChargeComboBox.SelectedItem = FindComboBoxItemByContent(InputPersonInChargeComboBox, selectedTask.Assignee);
            DeleteButton.IsEnabled = true;
        }
    }

    private object FindComboBoxItemByContent(ComboBox comboBox, string content)
    {
        foreach (var item in comboBox.Items)
        {
            if (item is ComboBoxItem comboBoxItem && comboBoxItem.Content.ToString() == content)
            {
                return comboBoxItem;
            }
            else if (item is string str && str == content)
            {
                return str;
            }
        }
        return null;
    }

    void Save()
    {
        List<string> Assignees = [];
        foreach (object item in InputPersonInChargeComboBox.Items)
        {
            if (item is ComboBoxItem cbItem)
            {
                Assignees.Add($"{cbItem.Content}");
            } else if (item is string txtItem)
            {
                Assignees.Add(txtItem);
            }
        }

        ProjectPlanningSaveFormat ppsf = new()
        {
            Tasks = TaskList.Items.Cast<Task>().ToList(),
            Assignees = Assignees
        };
        string json = JsonSerializer.Serialize(ppsf, new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });

        string directoryPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string targetDir = Path.Combine(directoryPath, ".p3");
        if (!Directory.Exists(targetDir))
        {
            Directory.CreateDirectory(targetDir);
            // 隠し属性を設定
            DirectoryInfo dirInfo = new(targetDir);
            dirInfo.Attributes |= FileAttributes.Hidden;
        }
        string filePath = Path.Combine(targetDir, "ProjectPlanning.json");
        _ = File.WriteAllTextAsync(filePath, json);
    }

    async void Load()
    {
        string directoryPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string targetDir = Path.Combine(directoryPath, ".p3");
        string filePath = Path.Combine(targetDir, "ProjectPlanning.json");
        if (File.Exists(filePath))
        {
            string json = await File.ReadAllTextAsync(filePath);
            ProjectPlanningSaveFormat ppsf = JsonSerializer.Deserialize<ProjectPlanningSaveFormat>(json);
            if (ppsf != null && ppsf.Tasks != null)
            {
                TaskList.Items.Clear();
                foreach (Task task in ppsf.Tasks)
                {
                    TaskList.Items.Add(task);
                }

                InputPersonInChargeComboBox.Items.Clear();
                foreach (string assignee in ppsf.Assignees)
                {
                    InputPersonInChargeComboBox.Items.Add(assignee);
                }
            }
        }
        else
        {
            ProjectPlanningSaveFormat ppsf = new()
            {
                Tasks = [],
                Assignees = []
            };
            string json = JsonSerializer.Serialize(ppsf, new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
            Directory.CreateDirectory(targetDir);
            // 隠し属性を設定
            DirectoryInfo dirInfo = new(targetDir);
            dirInfo.Attributes |= FileAttributes.Hidden;
            _ = File.WriteAllTextAsync(filePath, json);
        }
    }

    async void EditAssignessButton_Click(object sender, RoutedEventArgs e)
    {
        ListView Assignees = new();
        foreach (object item in InputPersonInChargeComboBox.Items)
        {
            if (item is ComboBoxItem comboBoxItem)
            {
                Assignees.Items.Add($"{comboBoxItem.Content}");
            } else if (item is string textItem) {
                Assignees.Items.Add(textItem);
            }
        }

        TextBox AssigneInputBox = new()
        {
            Margin = new Thickness(0, 8, 0, 0)
        };

        Button DeleteButton = new()
        {
            Content = DeleteText.Text,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(8, 0, 0, 0),
        };
        DeleteButton.Click += (s, e) =>
        {
            if (Assignees.SelectedItem != null)
            {
                Assignees.Items.Remove(Assignees.SelectedItem);
            }
        };

        Button AddButton = new()
        {
            Content = AddText.Text,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center
        };
        AddButton.Click += (s, e) =>
        {
            string newAssignee = AssigneInputBox.Text;
            if (!string.IsNullOrWhiteSpace(newAssignee))
            {
                Assignees.Items.Add(newAssignee);
                AssigneInputBox.Text = "";
            }
        };

        Grid Buttons = new()
        {
            ColumnDefinitions =
            {
                new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) }
            },
            Children =
            {
                AddButton,
                DeleteButton
            },
            Margin = new Thickness(0, 8, 0, 0),
        };
        Grid.SetColumn(DeleteButton, 1);

        ContentDialog dialog = new()
        {
            XamlRoot = Content.XamlRoot,
            Title = EditAssigneeText.Text,
            Content = new StackPanel()
            {
                Children =
                {
                    Assignees,
                    AssigneInputBox,
                    Buttons,
                }
            },
            PrimaryButtonText = "OK",
            CloseButtonText = CancelText.Text,
            DefaultButton = ContentDialogButton.Primary
        };

        ContentDialogResult result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            InputPersonInChargeComboBox.Items.Clear();
            foreach (string item in Assignees.Items)
            {
                InputPersonInChargeComboBox.Items.Add(item);
            }
            Save();
        }
    }
}