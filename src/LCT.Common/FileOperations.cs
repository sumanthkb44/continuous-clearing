﻿// --------------------------------------------------------------------------------------------------------------------
// SPDX-FileCopyrightText: 2023 Siemens AG
//
//  SPDX-License-Identifier: MIT

// -------------------------------------------------------------------------------------------------------------------- 

using CycloneDX.Models;
using LCT.Common.Interface;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;

namespace LCT.Common
{
    /// <summary>
    /// FileOperations class - responsible for writing , reading the file
    /// </summary>
    public class FileOperations : IFileOperations
    {
        static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void ValidateFilePath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException($"Invalid value for the {nameof(filePath)} - {filePath}");
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The {nameof(filePath)}  is not found at this path" +
                    $" - {filePath}");
            }
        }

        /// <summary>
        /// writes the content to the specified file
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataToWrite">dataToWrite</param>
        /// <param name="folderPath">folderPath</param>
        /// <param name="fileNameWithExtension">fileNameWithExtension</param>
        /// <param name="projectName">projectName</param>
        public string WriteContentToFile<T>(T dataToWrite, string folderPath, string fileNameWithExtension, string projectName)
        {
            try
            {
                Logger.Debug($"WriteContentToFile():folderpath-{folderPath},fileNameWithExtension-{fileNameWithExtension}," +
                    $"projectName-{projectName}");
                string jsonString = JsonConvert.SerializeObject(dataToWrite, Formatting.Indented);
                string fileName = $"{projectName}_{fileNameWithExtension}";

                string filePath = Path.Combine(folderPath, fileName);
                Logger.Debug($"filePath-{filePath}");

                BackupTheGivenFile(folderPath, fileName);
                File.WriteAllText(filePath, jsonString);

            }
            catch (IOException e)
            {
                Logger.Debug($"WriteContentToFile():Error:", e);
                return "failure";
            }
            catch (UnauthorizedAccessException e)
            {
                Logger.Debug($"WriteContentToFile():Error:", e);
                return "failure";
            }
            catch (SecurityException e)
            {
                Logger.Debug($"WriteContentToFile():Error:", e);
                return "failure";
            }
            Logger.Debug($"WriteContentToFile():End");
            return "success";

        }

        public Bom CombineComponentsFromExistingBOM(Bom components, string filePath)
        {
            Bom comparisonData = new Bom();
            try
            {

                if (File.Exists(filePath))
                {
                    StreamReader fileRead = new StreamReader(filePath);
                    var content = fileRead.ReadToEnd();

                    comparisonData = JsonConvert.DeserializeObject<Bom>(content);
                    fileRead.Close();
                    List<Component> list = new List<Component>(comparisonData.Components.Count + components.Components.Count);
                    list.AddRange(comparisonData.Components);
                    list.AddRange(components.Components);
                    comparisonData.Components = list;
                    comparisonData.Components = comparisonData.Components?.GroupBy(x => new { x.Name, x.Version }).Select(y => y.First()).ToList();
                }
                else
                {
                    Logger.Error($"Error:Invalid path entered,Please check if the comparison BOM  path entered is correct");
                    throw new FileNotFoundException();
                }

            }
            catch (IOException e)
            {
                Environment.ExitCode = -1;
                Logger.Error($"Error:Invalid path entered,Please check if the comparison BOM  path entered is correct", e);
            }
            catch (UnauthorizedAccessException e)
            {
                Environment.ExitCode = -1;
                Logger.Error($"Error:Invalid path entered,Please check if the comparison BOM path entered is correct", e);
            }
            return comparisonData;
        }
        public string WriteContentToCycloneDXFile<T>(T dataToWrite,string filePath, string fileNameWithExtension)
        {
            try
            {
                Logger.Debug($"WriteContentToCycloneDXFile():folderpath-{filePath}");
                string jsonString = JsonConvert.SerializeObject(dataToWrite, Formatting.Indented);
                string filename = Path.GetFileName(fileNameWithExtension);
                filePath =$"{filePath}\\{filename}";
                File.Copy(fileNameWithExtension, filePath);
                File.WriteAllText(filePath, jsonString);

            }
            catch (IOException e)
            {
                Logger.Debug($"WriteContentToCycloneDXFile():Error:", e);
                return "failure";
            }
            catch (UnauthorizedAccessException e)
            {
                Logger.Debug($"WriteContentToCycloneDXFile():Error:", e);
                return "failure";
            }
            catch (SecurityException e)
            {
                Logger.Debug($"WriteContentToCycloneDXFile():Error:", e);
                return "failure";
            }
            Logger.Debug($"WriteContentToCycloneDXFile():End");
            return "success";

        }

        private static void BackupTheGivenFile(string folderPath, string fileName)
        {
            string oldFile = Path.Combine(folderPath, fileName);
            string newFile = string.Format("{0}/{1:MM-dd-yyyy_HHmm}_Backup_{2}", folderPath, DateTime.Now, fileName);
            Logger.Debug($"BackupTheGivenFile():oldFile{oldFile},newFile{newFile}");
            try
            {
                if (File.Exists(oldFile))
                {
                    File.Move(oldFile, newFile);
                    Logger.Debug($"BackupTheGivenFile():Successfully taken backup of {oldFile}");
                }
            }
            catch (IOException ex)
            {
                Logger.Debug($"BackupTheGivenFile():", ex);
            }
            catch (NotSupportedException ex)
            {
                Logger.Debug($"BackupTheGivenFile():", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Debug($"BackupTheGivenFile():", ex);
            }
        }
    }
}
