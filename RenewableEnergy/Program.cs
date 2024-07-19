﻿/****************************************************************************************
 * Project Name:     RenweableEnergy
 * File Name:        Program.cs
 * Date:             2024-07-18
 * Authors:          Ethan Rivers and Jefferson Gilbert 
 * Purpose:          Contains the main method for the RenewableEnergy project.
 * 
 * Description:
 * A console application in C# that uses an XML file (renewable-energy.xml) as a data source 
 * and generates parameterized reports based on user inputs. The XML file contains data about 
 * renewable electricity production in 2021 for 225 countries. The end user should be able to 
 * view three varieties of tabular reports: based on a selected country, based on a selected 
 * source of renewable energy, or based on countries where renewable energy production 
 * accounts for a certain percentage of all energy production. 
 * 
 ****************************************************************************************/
using System.Globalization;
using System.Xml;
using System.Xml.XPath;
namespace RenewableEnergy
{

    public class Program
    {
        const string XmlFile = @"..\..\..\..\renewable-electricity.xml"; // in the soultion folder
        static void Main(string[] args)
        {

            XmlDocument doc = new XmlDocument();
            XmlNode? rootNode;
            XmlNodeList? allCountryNodes = null; // this wil be populated using XPath
            string year;
           
            // Load the data from the XML file using the DOM
            try
            {
                doc.Load(XmlFile);
                if (doc.DocumentElement != null)
                {
                    // store important things in memory using XPath
                    rootNode = doc.DocumentElement;
                    year = rootNode?.SelectSingleNode("@year")?.Value ?? string.Empty;
                    allCountryNodes = rootNode?.SelectNodes("//country"); // obtain all country nodes using XPath

                    Console.WriteLine($"Renewable Electricity Production in {year}");
                    Console.WriteLine("========================================");

                    // this is where we can output the previous report from sthe settings xml file if it exists...

                    List<string> commands = new List<string> { "C", "S", "P", "X" };

                    bool quit = false;
                    while (!quit)
                    {
                        Console.Write("\nEnter 'C' to select a country, 'S' to select a specific source" +
                            ", 'P' to select\na % range of renewables production, or 'X' to quit: ");

                        string commandString = Console.ReadLine() ?? string.Empty;

                        if (commands.Contains(commandString.ToUpper()))
                        {
                            switch (commandString.ToUpper())
                            {
                                case "C":
                                    DisplayNumberedMenuOfCountries(allCountryNodes);
                                    bool reportFinished = false;
                                    string countryNumberStr;

                                    while (!reportFinished)
                                    {
                                        Console.Write("Enter a country #: ");
                                        countryNumberStr = Console.ReadLine() ?? string.Empty;
                                        if (int.TryParse(countryNumberStr, out int index))
                                        {
                                            if (index > 0 && index <= allCountryNodes?.Count)
                                            {
                                                // generate the report for the selected country...
                                                GenerateReportForCountry(allCountryNodes[index - 1]);
                                                reportFinished = true;
                                            }
                                            else
                                            {
                                                Console.WriteLine("Invalid Country Error: Please enter a valid country number...");
                                            }

                                        }
                                        else
                                        {
                                            Console.WriteLine("Invalid Country Error: Please enter a valid country number...");
                                        }
                                    }

                                    break;
                                case "S":
                                    break;
                                case "P":
                                    break;
                                case "X":
                                    quit = true;
                                    Console.WriteLine("Shutting down program...");
                                    break;
                            }
                        }
                        else { Console.WriteLine("Invalid Command Error: Please enter a valid command..."); }
                    } // end of program loop

                    // once user quits we will save their data to a file as per instructions...
                    // maybe we can use a method to save the data to a file...
                }
            }
            catch (XPathException e)
            {
                Console.WriteLine("Error selecting country nodes: " + e.Message);
                Console.WriteLine(e.StackTrace);
            }
            catch (XmlException e)
            {
                Console.WriteLine("Error parsing XML file: " + e.Message);
                Console.WriteLine(e.StackTrace);
                return;
            }
            catch (IOException e)
            {
                Console.WriteLine("Error reading XML file: " + e.Message);
                Console.WriteLine(e.StackTrace);
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine("An unexpected error occured: " + e.Message);
                Console.WriteLine(e.StackTrace);
                return;
            }
        }

        public static void DisplayNumberedMenuOfCountries(XmlNodeList? allCountries)
        {
            if (allCountries?.Count > 0)
            {
                int index = 0;
                int countriesPerLine = 3;
                int maxNameLength = 27; // for clean formatting in menu

                foreach (XmlNode countryNode in allCountries)
                {
                    if ((index) % countriesPerLine == 0 && index != 1)
                    {
                        Console.WriteLine();
                    }


                    // Use XPath to get the name attribute of the current country node
                    string countryName = countryNode.SelectSingleNode("@name")?.Value ?? string.Empty;

                    // just truncate the country name if it is too long to fit in the menu
                    if (countryName.Length > maxNameLength)
                        countryName = countryName.Substring(0, maxNameLength - 3) + "...";
                    
                    Console.Write("{0,3}. {1,-30}", ++index, countryName);
                }

                Console.WriteLine(); // Ensure the last line is properly terminated
            }
            Console.WriteLine();
        }

        public static void GenerateReportForCountry(XmlNode? countryNode)
        {
            // output the title of the report
            Console.WriteLine();
            string title = "Renewable Electricity Production in ";
            string countryName = countryNode?.SelectSingleNode("@name")?.Value ?? string.Empty;
            int hypenCount = title.Length + countryName.Length;
            Console.WriteLine($"{title}{countryName}");
            for (int i = 0; i < hypenCount; i++)
            {
                Console.Write("-");
                if (i == hypenCount - 1)
                    Console.WriteLine("\n");
            }

            // columns for the report. each row should not exceed 80 characters
            string units = countryNode?.SelectSingleNode("ancestor::renewable-electricity/@units")?.Value ?? string.Empty;

            

        }
    }
}
