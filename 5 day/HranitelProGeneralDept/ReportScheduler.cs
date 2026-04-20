using System;
using System.Data;
using System.IO;
using System.Timers;
using System.Windows;

namespace HranitelProGeneralDept
{
    public class ReportScheduler
    {
        private System.Timers.Timer timer;
        private DatabaseHelper db;
        private string reportsPath;

        public ReportScheduler()
        {
            db = new DatabaseHelper();
            CreateReportsDirectory();
            StartTimer();
        }

        private void CreateReportsDirectory()
        {
            // Папка в директории проекта
            string projectPath = AppDomain.CurrentDomain.BaseDirectory;
            reportsPath = Path.Combine(projectPath, "Отчеты ТБ");

            if (!Directory.Exists(reportsPath))
            {
                Directory.CreateDirectory(reportsPath);
            }
        }

        private void StartTimer()
        {
            // Запускаем таймер на каждые 3 часа
            timer = new System.Timers.Timer(3 * 60 * 60 * 1000); // 3 часа в миллисекундах
            timer.Elapsed += OnTimerElapsed;
            timer.AutoReset = true;
            timer.Start();

            // Сразу создаем отчет за текущий период
            GenerateAndSaveReport();
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            // Выполняем в UI потоке
            Application.Current.Dispatcher.Invoke(() =>
            {
                GenerateAndSaveReport();
            });
        }

        private void GenerateAndSaveReport()
        {
            try
            {
                // Определяем период (последние 3 часа)
                DateTime endTime = DateTime.Now;
                DateTime startTime = endTime.AddHours(-3);

                // Получаем данные
                var dt = db.GetVisitorsByDepartmentReport(startTime, endTime);

                // Создаем папку для сегодняшней даты
                string todayFolder = Path.Combine(reportsPath, DateTime.Now.ToString("dd_MM_yyyy"));
                if (!Directory.Exists(todayFolder))
                {
                    Directory.CreateDirectory(todayFolder);
                }

                // Формируем имя файла
                string fileName = $"Отчет_{DateTime.Now:HH_mm}_за_3_часа.csv";
                string filePath = Path.Combine(todayFolder, fileName);

                // Сохраняем отчет в CSV
                SaveReportToCsv(dt, filePath);

                // Логируем (опционально)
                File.AppendAllText(Path.Combine(reportsPath, "log.txt"),
                    $"{DateTime.Now}: Отчет сохранен - {fileName}\n");
            }
            catch (Exception ex)
            {
                File.AppendAllText(Path.Combine(reportsPath, "error_log.txt"),
                    $"{DateTime.Now}: Ошибка - {ex.Message}\n");
            }
        }

        private void SaveReportToCsv(DataTable dt, string filePath)
        {
            using (var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8))
            {
                // Заголовки
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    writer.Write(dt.Columns[i].ColumnName);
                    if (i < dt.Columns.Count - 1)
                        writer.Write(";");
                }
                writer.WriteLine();

                // Данные
                foreach (DataRow row in dt.Rows)
                {
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        writer.Write(row[i].ToString());
                        if (i < dt.Columns.Count - 1)
                            writer.Write(";");
                    }
                    writer.WriteLine();
                }
            }
        }

        public void StopTimer()
        {
            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
            }
        }
    }
}