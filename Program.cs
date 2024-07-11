class Program
{
    static async Task Main()
    {
        DirectoryInfo dirInfo = new DirectoryInfo("CSV");
        FileInfo[] csvFiles = dirInfo.GetFiles("*.csv");

        foreach (FileInfo csvFile in csvFiles)
        {
            Console.WriteLine($"Processing Split [./CSV/{csvFile.Name}] started...");
            await SplitCsvFile(csvFile.Name);
        }
    }

    static async Task SplitCsvFile(string inputFileName)
    {
        //1GBごとに分割
        float splitSizeInGB = 1.0f;
        //0.15GBほど余裕を持たせる
        float checkSizeInGB = splitSizeInGB - 0.15f;

        string csvFileName = Path.GetFileNameWithoutExtension(inputFileName);
        string inputFilePath = $"./CSV/{csvFileName}.csv";
        int outputFileNameSuffix = 1;
        string outputFileName = $"{csvFileName}_split_{outputFileNameSuffix}.csv";
        string outputFilePath = $"./SPLIT/{outputFileName}";

        try
        {
            //50万行ごとにチェック
            int checkCount = 5 * 10 * 1000;
            Console.WriteLine($"Spliting to [{outputFilePath}]");

            int count = 0;
            StreamWriter writer = new StreamWriter(outputFilePath, true);
            using (StreamReader reader = new StreamReader(inputFilePath))
            {
                var header = reader.ReadLine();
                writer.WriteLine(header);
                while (true)
                {
                    count++;
                    var line = await reader.ReadLineAsync();
                    if (line is null)
                    {
                        break;
                    }
                    await writer.WriteLineAsync(line);
                    if (count == checkCount)
                    {
                        var outputFileInfo = new FileInfo(outputFilePath);
                        float fileSizeInGB = (float)outputFileInfo.Length / (1024 * 1024 * 1024);
                        if (fileSizeInGB >= checkSizeInGB)
                        {
                            writer.Close();
                            outputFileNameSuffix++;
                            outputFileName = $"{csvFileName}_split_{outputFileNameSuffix}.csv";
                            outputFilePath = $"./SPLIT/{outputFileName}";
                            Console.WriteLine($"Spliting to [{outputFilePath}]");
                            writer = new StreamWriter(outputFilePath, true);
                            writer.WriteLine(header);
                        }
                        count = 0;
                    }
                }
            }
            Console.WriteLine($"Processing Split [./CSV/{inputFileName}] completed...");
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"An error occurred when processing [InputFilePath : {inputFilePath}] [OutputFilePath : {outputFilePath}]"
            );
            Console.WriteLine("Error Message : " + ex.Message);
        }
    }
}
