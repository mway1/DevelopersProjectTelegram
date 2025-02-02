﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json;
using TelegramTestBot.BL;
using TelegramTestBot.BL.Questions;
using Excel = Microsoft.Office.Interop.Excel;
using System.Reflection;

namespace TelegramTestBot.BL.Data
{
    public class TestsBase
    {
        private const string filePath = @"developersTB.tb";
        public List<Test> AllTests { get; private set; }

        private static TestsBase _instance;

        private TestsBase()
        {
            AllTests = new List<Test>();
        }

        public static TestsBase GetInstance()
        {
            if (_instance == null)
            {
                _instance = new TestsBase();
            }
            return _instance;
        }

        private string Serialize(List<Test> tests)
        {
            return JsonSerializer.Serialize<List<Test>>(tests);
        }

        private List<Test> Deserialize(string json)
        {
            return JsonSerializer.Deserialize<List<Test>>(json);
        }

        public void Save(List<Test> tests)
        {
            string json = Serialize(tests);

            using (StreamWriter sw = new StreamWriter(filePath, false))
            {
                sw.WriteLine(json);
            }
        }

        public void Load()
        {
            using (StreamReader sr = new StreamReader(filePath))
            {
                string json = sr.ReadLine();
                AllTests = Deserialize(json);
            }
        }

        public void CreateTestReport(string nameOfGroup, Test currentTest)
        {
            string reportName = $"Отчёт по группе {nameOfGroup}, тест: {currentTest.NameTest}";
            List<string> usersNames = new List<string> { };
            List<List<string>> allAnswersOfUsers = new List<List<string>>(usersNames.Count);
            if (BaseOfUsers.GroupBase.ContainsKey(nameOfGroup))
            {
                foreach (var users in BaseOfUsers.NameBase)
                {
                    if (BaseOfUsers.GroupBase[nameOfGroup].Contains(users.Value))
                    {
                        usersNames.Add(users.Value);
                        if (BaseOfUsers.UserAnswers.Keys.Count != 0)
                        {
                            allAnswersOfUsers.Add(BaseOfUsers.UserAnswers[users.Key]);
                        }
                    }
                }
            }
            Excel.Application oXL;
            Excel._Workbook report;
            Excel._Worksheet oSheet;
            Excel.Range oRng;
            oXL = new Excel.Application();
            oXL.Visible = true;
            oXL.SheetsInNewWorkbook = 2;
            report = (Excel._Workbook)(oXL.Workbooks.Add(Missing.Value));
            oSheet = (Excel._Worksheet)report.Worksheets[1];
            oSheet.Cells[1, 1] = $"{reportName}";
            oSheet.Name = $"{currentTest.NameTest}";

            int schetchik = 0;
            foreach(var user in usersNames)
            {
                oSheet.Cells[schetchik + 2, 1] = $"{usersNames[schetchik]}";
                schetchik++;

            }
            schetchik = 0;
         
            for (int i = 0; i < currentTest.Questions.Count; i++)
            {
                string currentQuestion = "";
                currentQuestion = currentTest.Questions[i].ContentOfQuestion;

                oSheet.Cells[1,i+2] = $"{currentQuestion}";
            }
            for (int i = 0; i < usersNames.Count; i++)
            {
                for(int j = 0; j < currentTest.Questions.Count; j++)
                {                    
                    if (j<allAnswersOfUsers[i].Count)
                    {
                        oSheet.Cells[i + 2, j + 2] = $"{allAnswersOfUsers[i][j]}";

                        if (currentTest.Questions[j].TypeOfQuestion == 3)
                        {
                            oSheet.Cells[i + 2, j + 2].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Gray);
                        }
                        else if (allAnswersOfUsers[i][j] == currentTest.Questions[j].CorrectAnswer)
                        {
                            oSheet.Cells[i + 2, j + 2].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.LightGreen);
                        }
                        else if (allAnswersOfUsers[i][j] != currentTest.Questions[j].CorrectAnswer)
                        {
                            oSheet.Cells[i + 2, j + 2].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Red);
                        }
                    }
                    else
                    {
                        oSheet.Cells[i + 2, j + 2] = "Нет ответа";
                    }
                }
            }
            oSheet.Columns.AutoFit();
        }
    }
}