/****************************************************************************************
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
using System.Text;
using System.Xml;
using System.Xml.XPath;
namespace RenewableEnergy
{

    public class Program
    {
        const string XmlFile = @"..\..\..\..\renewable-electricity.xml"; // in the soultion folder
        const string SettingsFile = @"..\..\..\..\settings.xml"; // settings file path in solution folder
        static void Main(string[] args)
        {

            XmlDocument doc = new XmlDocument();
            XmlNode? rootNode;
            XmlNodeList? allCountryNodes = null; // this wil be populated using XPath.
            string year;

            XmlDocument settingsDoc = new XmlDocument(); 
            XmlElement? settingsRoot = null;

            // some simple string variables to hold user type, selection, and range
            string globalType = string.Empty;
            string globalSelection = string.Empty;
            string globalMinPercent = string.Empty;
            string globalMaxPercent = string.Empty;

            // Load the data from the XML file using the DOM
            try
            {
                doc.Load(XmlFile);
                if (doc.DocumentElement != null)
                {
                    // store important things in memory using XPath
                    rootNode = doc.SelectSingleNode("/");
                    year = rootNode?.SelectSingleNode("//@year")?.Value ?? string.Empty;
                    allCountryNodes = rootNode?.SelectNodes("//country"); // obtain all country nodes using XPath

                    

                    Console.OutputEncoding = Encoding.UTF8; // change console output to view copyright symbol
                    Console.WriteLine("XML Report Generator \u00A9 Copyright 2024 ~ Ethan Rivers & Jefferson Gilbert\n\n");
                    Console.WriteLine($"Renewable Electricity Production in {year}");

                    Console.WriteLine("========================================");

                    // Try to load previous query from the settings file 
                    try
                    {
                        settingsDoc.Load(SettingsFile);
                        settingsRoot = settingsDoc.DocumentElement;

                        Console.WriteLine();
                        Console.WriteLine("Here is the final report you requested the last time you were here...\n");
                        // now we just generate report base on type
                        string type = settingsRoot?.SelectSingleNode("type")?.InnerText ?? string.Empty;
                        string selection = settingsRoot?.SelectSingleNode("selection")?.InnerText ?? string.Empty;
                        string min = settingsRoot?.SelectSingleNode("min")?.InnerText ?? string.Empty;
                        string max = settingsRoot?.SelectSingleNode("max")?.InnerText ?? string.Empty;

                        if (type == "C")
                        {
                            GenerateReportForCountry(rootNode?.SelectSingleNode($"//country[@name=\"{selection}\"]"));
                        }
                        else if (type == "S")
                        {
                            GenerateReportForSpecificTypeOfRenewableEnergy(rootNode, selection);
                        }
                        else if (type == "P")
                        {
                            GenerateRangeBasedReport(rootNode, double.Parse(min), double.Parse(max));
                        }
                    }
                    catch (Exception)
                    {
                        // If loading fails, initialize a new settings document
                        settingsDoc = new XmlDocument();
                        XmlDeclaration declNode = settingsDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
                        settingsDoc.AppendChild(declNode);
                        settingsRoot = settingsDoc.CreateElement("settings");
                        settingsDoc.AppendChild(settingsRoot);


                    }



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
                                                GenerateReportForCountry(allCountryNodes[index - 1]); // XPath for this was done on line 43

                                                // store the user's selection in memory and save to setting file
                                                globalType = "C";
                                                globalSelection = allCountryNodes[index - 1]?.SelectSingleNode("@name")?.Value ?? string.Empty;
                                                globalMinPercent = "-1"; // default value
                                                globalMaxPercent = "-1"; // default value
                                                SaveSettings(settingsDoc, settingsRoot, globalType, globalSelection, globalMinPercent, globalMaxPercent);

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

                                    // report is done. save settings to a xml file...


                                    break;
                                case "S":
                                    {
                                        // generate list of all types of energy sources using XPath
                                        XmlNodeList? typesOfRenewables = rootNode?.SelectNodes("//source/@type");
                                        if (typesOfRenewables is null)
                                        {
                                            Console.WriteLine("No renewable energy types found in the data.");
                                            break;
                                        }

                                        // remove all duplicates 
                                        HashSet<string> uniqueTypes = new();
                                        foreach (XmlNode type in typesOfRenewables)
                                        {
                                            uniqueTypes.Add(type.Value ?? "");
                                        }

                                        Console.WriteLine("\nSelect a renewable by number as shown below...");
                                        for (int i = 0; i < uniqueTypes.Count; i++)
                                        {
                                            Console.WriteLine($"  {i + 1}. {uniqueTypes.ElementAt(i)}");
                                        }
                                        Console.WriteLine();
                                        Console.WriteLine();

                                        string renewableNumberStr;
                                        bool validIndex = false;
                                        while (!validIndex)
                                        {
                                            Console.Write("Enter a renewable #: ");
                                            renewableNumberStr = Console.ReadLine() ?? string.Empty;
                                            if (int.TryParse(renewableNumberStr, out int index))
                                            {
                                                if (index > 0 && index <= uniqueTypes.Count)
                                                {
                                                    string typeStr = uniqueTypes.ElementAt(index - 1);
                                                    GenerateReportForSpecificTypeOfRenewableEnergy(rootNode, typeStr);
                                                    globalType = "S";
                                                    globalSelection = typeStr;
                                                    globalMinPercent = "-1"; // default value
                                                    globalMaxPercent = "-1"; // default value
                                                    SaveSettings(settingsDoc, settingsRoot, globalType, globalSelection, globalMinPercent, globalMaxPercent);
                                                    validIndex = true;
                                                }
                                                else
                                                {
                                                    Console.WriteLine("Invalid Renewable Error: Please enter a valid renewable number...");
                                                }
                                            }
                                            else
                                            {
                                                Console.WriteLine("Invalid Renewable Error: Please enter a valid renewable number...");
                                            }
                                        }
                                    }
                                    break;
                                case "P":
                                    bool validInput = false;
                                    while (!validInput)
                                    {
                                        double min = -1, max = -1; // start these of at our default values
                                        bool isMinDefault = false, isMaxDefault = false; // these mark if the user didn't enter a value for min or max

                                        // Get user input for minimum and maximum range
                                        Console.WriteLine();
                                        Console.Write("Enter the minimum % of renewables produced or press enter for no minimum: ");
                                        string minStr = Console.ReadLine() ?? string.Empty;
                                        Console.Write("Enter the maximum % of renewables produced or press enter for no maximum: ");
                                        string maxStr = Console.ReadLine() ?? string.Empty;

                                        // Handle empty inputs
                                        if (!string.IsNullOrEmpty(minStr) && !double.TryParse(minStr, out min))
                                        {
                                            Console.WriteLine("Invalid Range Error: Please enter a valid number for the minimum value...");
                                            continue;
                                        }

                                        if (!string.IsNullOrEmpty(maxStr) && !double.TryParse(maxStr, out max))
                                        {
                                            Console.WriteLine("Invalid Range Error: Please enter a valid number for the maximum value...");
                                            continue;
                                        }

                                        // If both inputs are empty, set them to -1 and mark defaults
                                        if (string.IsNullOrEmpty(minStr))
                                        {
                                            min = -1;
                                            isMinDefault = true;
                                        }

                                        if (string.IsNullOrEmpty(maxStr))
                                        {
                                            max = -1;
                                            isMaxDefault = true;
                                        }

                                        // If both are valid numbers but min is greater than max, restart the loop
                                        if (min > max && !isMinDefault && !isMaxDefault)
                                        {
                                            Console.WriteLine("Invalid Range Error: The minimum value cannot be greater than the maximum value...");
                                            continue;
                                        }

                                        // Check if the numbers are within the range 0-100 if not defaults
                                        if ((min < 0 || min > 100) && !isMinDefault)
                                        {
                                            Console.WriteLine("Range Error: The minimum value must be between 0 and 100...");
                                            continue;
                                        }

                                        if ((max < 0 || max > 100) && !isMaxDefault)
                                        {
                                            Console.WriteLine("Range Error: The maximum value must be between 0 and 100...");
                                            continue;
                                        }

                                        GenerateRangeBasedReport(rootNode, min, max);
                                        globalType = "P";
                                        globalSelection = "-1"; // default value
                                        globalMinPercent = min.ToString();
                                        globalMaxPercent = max.ToString();
                                        SaveSettings(settingsDoc, settingsRoot, globalType, globalSelection, globalMinPercent, globalMaxPercent);
                                        validInput = true;
                                    }
                                    break;
                                case "X":
                                    quit = true;
                                    Console.WriteLine("\nShutting down program...");
                                    break;
                            }
                        }
                        else { Console.WriteLine("Invalid Command Error: Please enter a valid command..."); }
                    } // end of program loop

                } // end of if block for checking if the root node is not null
            } // end of try block for loading the XML file
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

        private static void SaveSettings(XmlDocument settingsDoc, XmlElement? settingsRoot, string type, string selection, string min, string max)
        {
            // clear the settings file
            settingsRoot?.RemoveAll();

            // create the elements for the settings file
            XmlElement typeElement = settingsDoc.CreateElement("type");
            typeElement.InnerText = type;
            settingsRoot?.AppendChild(typeElement);

            XmlElement selectionElement = settingsDoc.CreateElement("selection");
            selectionElement.InnerText = selection;
            settingsRoot?.AppendChild(selectionElement);

            XmlElement minElement = settingsDoc.CreateElement("min");
            minElement.InnerText = min;
            settingsRoot?.AppendChild(minElement);

            XmlElement maxElement = settingsDoc.CreateElement("max");
            maxElement.InnerText = max;
            settingsRoot?.AppendChild(maxElement);

            // save the settings file
            settingsDoc.Save(SettingsFile);

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
            string[] columnHeaders = { "Renewable Type", $"Amount ({units})", "% of Total", "% of Renewables" };
            int matchesFound;

            // formatted report within 80 characters
            Console.WriteLine(" {0,-18} {1,-18} {2,-11} {3,-5}", columnHeaders[0], columnHeaders[1], columnHeaders[2], columnHeaders[3]);
            Console.WriteLine();

            // get all the renewable nodes for the country going down the tree.  (like in class use descendant)
            XmlNodeList? renewableNodes = countryNode?.SelectNodes("descendant::source");
            matchesFound = renewableNodes?.Count ?? 0;


            for (int i = 0; i < matchesFound; i++)
            {
                // get report details using xpath
                string renewableType = renewableNodes?[i]?.SelectSingleNode("@type")?.Value ?? string.Empty;
                string amount = renewableNodes?[i]?.SelectSingleNode("@amount")?.Value ?? string.Empty;
                string percentOfAll = renewableNodes?[i]?.SelectSingleNode("@percent-of-all")?.Value ?? string.Empty;
                string percentOfRenewables = renewableNodes?[i]?.SelectSingleNode("@percent-of-renewables")?.Value ?? string.Empty;

                Console.WriteLine(" {0,14} {1,16} {2,16} {3,16}", renewableType, ApplyCommasToNumberStr(amount), percentOfAll, percentOfRenewables);
            }
            Console.WriteLine();

            Console.WriteLine(matchesFound + " match(es) found.");

        }

        public static void GenerateReportForSpecificTypeOfRenewableEnergy(XmlNode? root, string type)
        {
            if (root is null) { return; }

            string formattedTypeStr = type.Substring(0, 1).ToUpper() + type.Substring(1);
            string title = formattedTypeStr + " Electricity Production";

            Console.WriteLine();
            Console.WriteLine(formattedTypeStr + " Electricity Production");
            for (int i = 0; i < title.Length; i++)
            {
                Console.Write("-");
                if (i == title.Length - 1)
                    Console.WriteLine("\n");
            }

            XmlNodeList? countriesWithRenewable = root?.SelectNodes($"//country/source[@type=\"{type}\"]");
            if (countriesWithRenewable is null)
            {
                Console.WriteLine("No countries found with the selected renewable type.");
                return;
            }
            string units = root?.SelectSingleNode("//@units")?.Value ?? string.Empty;

            Console.WriteLine("{0,32} {1,14} {2,16} {3,16}", "Country", $"Amount ({units})", "% of Total", "% of Renewables");
            Console.WriteLine();
            int matchesFound = countriesWithRenewable.Count;
            for (int i = 0; i < matchesFound; i++)
            {
                string countryName = countriesWithRenewable[i]?.SelectSingleNode("ancestor::country/@name")?.Value ?? string.Empty;
                string amount = countriesWithRenewable[i]?.SelectSingleNode("@amount")?.Value ?? string.Empty;
                string percentOfAll = countriesWithRenewable[i]?.SelectSingleNode("@percent-of-all")?.Value ?? string.Empty;
                string percentOfRenewables = countriesWithRenewable[i]?.SelectSingleNode("@percent-of-renewables")?.Value ?? string.Empty;

                if (countryName.Length > 30)
                    countryName = countryName.Substring(0, 27) + "...";

                Console.WriteLine(" {0,31} {1,14} {2,16} {3,16}", countryName, ApplyCommasToNumberStr(amount), percentOfAll, percentOfRenewables);
            }
            Console.WriteLine($"{matchesFound} match(es) found.");
        }

        public static void GenerateRangeBasedReport(XmlNode? root, double min, double max)
        {
            if (root is null) { return; }

            XmlNodeList? filteredCountries = null;
            bool minGiven = min != -1;
            bool maxGiven = max != -1;

            if (minGiven && maxGiven)
                filteredCountries = root.SelectNodes($"//country[totals/@renewable-percent >= {min} and totals/@renewable-percent <= {max}]");
            else if (minGiven && !maxGiven)
                filteredCountries = root.SelectNodes($"//country[totals/@renewable-percent >= {min}]");
            else if (!minGiven && maxGiven)
                filteredCountries = root.SelectNodes($"//country[totals/@renewable-percent <= {max}]");
            else
            {
                // just get every last country and output their attributes if no range is given
                filteredCountries = root.SelectNodes("//country");

                Console.WriteLine();
                Console.WriteLine("Combined Renewables for All Countries");
                Console.WriteLine("-------------------------------------\n");
                Console.WriteLine("{0,33} {1,16} {2,16} {3,17}", "Country", $"All Elec. ({root?.SelectSingleNode("//@units")?.Value})", $"Renewable ({root?.SelectSingleNode("//@units")?.Value})", "% Renewable");
                Console.WriteLine();
                foreach (XmlNode country in filteredCountries!)
                {
                    string countryName = country.SelectSingleNode("@name")?.Value ?? string.Empty;
                    string allSources = country.SelectSingleNode("totals/@all-sources")?.Value ?? string.Empty;
                    string allRenewables = country.SelectSingleNode("totals/@all-renewables")?.Value ?? string.Empty;
                    string percentRenewable = country.SelectSingleNode("totals/@renewable-percent")?.Value ?? string.Empty;
                    if (countryName.Length > 30)
                        countryName = countryName.Substring(0, 27) + "...";
                    Console.WriteLine("{0,33} {1,16} {2,16} {3,17}", countryName, ApplyCommasToNumberStr(allSources), ApplyCommasToNumberStr(allRenewables), percentRenewable);
                }

                Console.WriteLine($"{filteredCountries.Count} match(es) found.");
                return;
            }

            if (filteredCountries is null) { return; }

            // output the header for the report
            Console.WriteLine();

            if (minGiven && maxGiven)
            {
                string[] header1 = { "Countries Where Renewables Account for", $"{min:F2}%", "to", $"{max:F2}%", "of Electricity Generation" };
                Console.WriteLine("{0} {1} {2} {3} {4}", header1[0], header1[1], header1[2], header1[3], header1[4]);
                // calculate length of header for underscores
                int headerLength = header1[0].Length + header1[1].Length + header1[2].Length + header1[3].Length + header1[4].Length + 4; // 4 accounts for spaces
                for (int i = 0; i < headerLength; i++)
                {
                    Console.Write("-");
                    if (i == headerLength - 1)
                        Console.WriteLine("\n");
                }
            }

            else if (minGiven && !maxGiven)
            {
                string[] header2 = { "Countries Where Renewables Account for At Least", $"{min:F2}%", "of Electricity Generation" };
                Console.WriteLine("{0} {1} {2}", header2[0], header2[1], header2[2]);
                // calculate length of header for underscores
                int headerLength = header2[0].Length + header2[1].Length + header2[2].Length + 2; // 2 accounts for spaces
                for (int i = 0; i < headerLength; i++)
                {
                    Console.Write("-");
                    if (i == headerLength - 1)
                        Console.WriteLine("\n");
                }
            }

            else if (!minGiven && maxGiven)
            {
                string[] header3 = { "Countries Where Renewables Account for Up To", $"{max:F2}%", "of Electricity Generation" };
                Console.WriteLine("{0} {1} {2}", header3[0], header3[1], header3[2]);
                // calculate length of header for underscores
                int headerLength = header3[0].Length + header3[1].Length + header3[2].Length + 2; // 2 accounts for spaces
                for (int i = 0; i < headerLength; i++)
                {
                    Console.Write("-");
                    if (i == headerLength - 1)
                        Console.WriteLine("\n");
                }
            }

            Console.WriteLine("{0,33} {1,16} {2,16} {3,17}", "Country", $"All Elec. ({root?.SelectSingleNode("//@units")?.Value})", $"Renewable ({root?.SelectSingleNode("//@units")?.Value})", "% Renewable");
            Console.WriteLine();

            foreach (XmlNode country in filteredCountries)
            {
                string countryName = country.SelectSingleNode("@name")?.Value ?? string.Empty;
                string allSources = country.SelectSingleNode("totals/@all-sources")?.Value ?? string.Empty;
                string allRenewables = country.SelectSingleNode("totals/@all-renewables")?.Value ?? string.Empty;
                string percentRenewable = country.SelectSingleNode("totals/@renewable-percent")?.Value ?? string.Empty;
                if (countryName.Length > 30)
                    countryName = countryName.Substring(0, 27) + "...";
                Console.WriteLine("{0,33} {1,16} {2,16} {3,17}", countryName, ApplyCommasToNumberStr(allSources), ApplyCommasToNumberStr(allRenewables), percentRenewable);
            }

            Console.WriteLine($"{filteredCountries.Count} match(es) found.");
        }

        // quick helper method to apply commas to numbers in string format
        public static string ApplyCommasToNumberStr(string number)
        {
            // Split the number into integer and decimal parts
            string[] parts = number.Split('.');
            string integerPart = parts[0];
            string decimalPart = parts.Length > 1 ? parts[1] : string.Empty;

            // Apply commas to the integer part
            string result = "";
            int count = 0;
            for (int i = integerPart.Length - 1; i >= 0; i--)
            {
                result = integerPart[i] + result;
                count++;
                if (count % 3 == 0 && i != 0)
                {
                    result = "," + result;
                }
            }

            // Combine the integer part with the decimal part
            if (!string.IsNullOrEmpty(decimalPart))
            {
                result = result + "." + decimalPart;
            }

            return result;
        }


    } // end of class
}
