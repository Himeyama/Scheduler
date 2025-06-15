using System;
using Common;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.Networking.NetworkOperators;
using Windows.System;
using System.Collections.Generic;


namespace Scheduler;

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

        // AddInputRow(); // 初期の入力行を追加
        InputStartPlanDatePicker.SelectedDate = DateTime.Now;
        InputEndPlanDatePicker.SelectedDate = DateTime.Now;
    }

    void AutoSave_Toggled(object sender, RoutedEventArgs e)
    {
        try
        {
            EnableAutoSave.Visibility = AutoSave.IsOn ? Visibility.Visible : Visibility.Collapsed;
            DisableAutoSave.Visibility = AutoSave.IsOn ? Visibility.Collapsed : Visibility.Visible;
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
            Task task = new()
            {
                Name = taskName,
                StartDate = InputStartDatePicker.SelectedDate?.DateTime,
                EndDate = InputEndDatePicker.SelectedDate?.DateTime,
                StartPlanDate = InputStartPlanDatePicker.SelectedDate?.DateTime ?? DateTime.Now,
                EndPlanDate = InputEndPlanDatePicker.SelectedDate?.DateTime ?? DateTime.Now,
                Priority = $"{(InputPriorityComboBox.SelectedItem as ComboBoxItem)?.Content ?? ""}",
                Assignee = $"{(InputPersonInChargeComboBox.SelectedItem as ComboBoxItem)?.Content ?? ""}",
                Status = $"{(InputStatusComboBox.SelectedItem as ComboBoxItem)?.Content ?? ""}"
            };

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
        }
        catch (Exception ex)
        {
            System.IO.File.AppendAllText("debug.txt", ex.Message + Environment.NewLine);
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
            InputPriorityComboBox.SelectedItem = FindComboBoxItemByContent(InputPriorityComboBox, selectedTask.Priority);
            InputStatusComboBox.SelectedItem = FindComboBoxItemByContent(InputStatusComboBox, selectedTask.Status);
            InputPersonInChargeComboBox.SelectedItem = FindComboBoxItemByContent(InputPersonInChargeComboBox, selectedTask.Assignee);
            DeleteButton.IsEnabled = true;
        }
    }

    private ComboBoxItem FindComboBoxItemByContent(ComboBox comboBox, string content)
    {
        foreach (var item in comboBox.Items)
        {
            if (item is ComboBoxItem comboBoxItem && comboBoxItem.Content.ToString() == content)
            {
                return comboBoxItem;
            }
        }
        return null;
    }
}