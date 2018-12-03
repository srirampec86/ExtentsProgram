using System;
using System.IO;
using System.Threading;
using System.Configuration;




namespace GetpointsCount
{
    class Program
    {
        static void Main(string[] args)
        {
            char extentsDivider = ' ';
            int minRangeValue = int.MinValue;
            int maxRangeValue = int.MaxValue;
            int numberOfRecords = Convert.ToInt32(ConfigurationManager.AppSettings["NO_OF_RECORDS"]);

            GenerateFiles.GenerateExtents(extentsDivider, numberOfRecords, minRangeValue, maxRangeValue);
            GenerateFiles.GeneratePoints(numberOfRecords, minRangeValue, maxRangeValue);

            var reader = new StreamReader(ConfigurationManager.AppSettings["EXTENTS_FILE_PATH"]);

            var rootNode = new RangeNode(0, 0);

            int counter = 0;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                counter += 1;
    
                string[] numbers = line.Split(extentsDivider);
                var node = new RangeNode(Convert.ToInt32(numbers[0]), Convert.ToInt32(numbers[1]));

                if (rootNode != null)
                    Helper.AddNode(rootNode, node);
                else
                    rootNode = node;
            }

            string[] pointsFile = File.ReadAllLines(ConfigurationManager.AppSettings["POINTS_FILE_PATH"]);

            StreamWriter writer = new StreamWriter(ConfigurationManager.AppSettings["OUTCOME_FILE_PATH"], true);
            counter = 0;
            foreach(var line in pointsFile)
            {
                counter += 1;
                int value = Convert.ToInt32(line);
                int count = Helper.GetRangeCount(value, rootNode);
                writer.WriteLine(string.Format("{0} {1} {2}", counter, value, count));
            }
            writer.Flush();
            writer.Close();
   
            Console.ReadKey();
        }

        public class Helper
        {

            //Forms a Tree structure for traversing to access Extents
            public static void AddNode(RangeNode node, RangeNode addNode)
            {
                if (node.MaxRightValue < addNode.MaxValue)
                    node.MaxRightValue = addNode.MaxValue;

                if (node.MinLeftValue > addNode.MinValue)
                    node.MinLeftValue = addNode.MinLeftValue;

                if (addNode.MinValue < node.MinValue)
                {
                    if (node.Left == null)
                        node.Left = addNode;
                    else
                        AddNode(node.Left, addNode);
                }
                else
                {
                    if (node.Right == null)
                        node.Right = addNode;
                    else
                        AddNode(node.Right, addNode);
                }
            }

            //Returns the Count of Points between the Extents
            public static int GetRangeCount(int value, RangeNode node)
            {
                if (node == null || node.MaxRightValue < value ||node.MinLeftValue > value)
                    return 0;
                else if (node.MinValue > value)
                    return GetRangeCount(value, node.Left);
                else
                {
                    int result = 0;

                    if (value <= node.MaxValue)
                        result = 1;

                    return result + GetRangeCount(value, node.Right) + GetRangeCount(value, node.Left);
                }
            }
        }

    }

    public class RangeNode
    {
        public RangeNode(int minValue, int maxValue)
        {
            this.MinValue = minValue;
            this.MaxValue = maxValue;
            this.MaxRightValue = maxValue;
            this.MinLeftValue = minValue;
        }
        public int MinValue { get; private set; }
        public int MaxValue { get; private set; }
        public int MaxRightValue { get; set; }
        public int MinLeftValue { get; set; }

        public RangeNode Left { get; set; }
        public RangeNode Right { get; set; }
    }


    public class GenerateFiles
    {
        //Generate millions of records randomly for Extents
        public static void GenerateExtents(char deviderm, int count, int minRangeValue, int maxRangeValue)
        {
            Random rnd = new Random();

            StreamWriter writer = new StreamWriter(ConfigurationManager.AppSettings["EXTENTS_FILE_PATH"], true);

            for (int i = 0; i < count; i++)
            {
                int minValue = rnd.Next(minRangeValue, maxRangeValue);
                writer.WriteLine(string.Format("{0}{1}{2}", minValue, deviderm, rnd.Next(minValue, maxRangeValue)));
            }
            writer.Flush();
            writer.Close();
            
        }

        //Generate millions of records randomly for Points
        public static void GeneratePoints(int count, int minRangeValue, int maxRangeValue)
        {
            Random rnd = new Random();

            StreamWriter writer = new StreamWriter(ConfigurationManager.AppSettings["POINTS_FILE_PATH"], true);
            for (int i = 0; i < count; i++)
            {
                writer.WriteLine(rnd.Next(minRangeValue, maxRangeValue).ToString());
            }
            writer.Flush();
            writer.Close();
        }

    }
}
