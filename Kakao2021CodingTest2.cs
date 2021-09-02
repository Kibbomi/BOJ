using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;

namespace APITest
{
    #region struct & class
    class Block
    {
        public Block()
        {
            ID = -1;
            bicycle = 0;
        }
        public Block(int ID_, int bicycle_)
        {
            ID = ID_;
            bicycle = bicycle_;
        }
        public int ID { get; set; }
        public int bicycle {get;set;}
    }

    struct Truck
    {
        public Truck(int ID_, int locationID_, int loaded_bikes_count_)
        {
            ID = ID_;
            locationID = locationID_;
            loaded_bikes_count = loaded_bikes_count_;
        }
        public int ID { get; set; }
        public int locationID { get; set; }
        public int loaded_bikes_count { get; set; }
    }
    #endregion

    #region comparer

    class ascendingComp : IComparer<Block>
    {
        public int Compare(Block lhs, Block rhs)
        {
            if (lhs.bicycle < rhs.bicycle)
                return -1;
            else if (lhs.bicycle == rhs.bicycle)
                return 0;
            else
                return 1;
        }
    }

    class descendingComp : IComparer<Block>
    {
        public int Compare(Block lhs, Block rhs)
        {
            if (lhs.bicycle < rhs.bicycle)
                return 1;
            else if (lhs.bicycle == rhs.bicycle)
                return 0;
            else
                return -1;
        }
    }

    #endregion


    class Program
    {
        #region APIs

        //get token
        static void StartAPI()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://kox947ka1a.execute-api.ap-northeast-2.amazonaws.com/prod/users/start");

            request.Method = "POST";
            request.ContentType = "application/json";
            request.Headers.Add("X-Auth-Token", "949e02e2d4ac45471825eec97affa22d");

            JObject item = new JObject();
            item.Add("problem", 1);


            StreamWriter reqStream = new StreamWriter(request.GetRequestStream());
            reqStream.Write(item);
            reqStream.Close();

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader respStream = new StreamReader(response.GetResponseStream());
                string resp = respStream.ReadToEnd();
                respStream.Close();

                JObject ret = JObject.Parse(resp);

                Console.WriteLine($"auth_key : {ret["auth_key"]}");
                Console.WriteLine($"problem : {ret["problem"]}");
                Console.WriteLine($"time : {ret["time"]}");

                FileStream s = new FileStream("apikey.txt", FileMode.Create);
                
                StreamWriter sw = new StreamWriter(s);
                sw.WriteLine(ret["auth_key"].ToString());

                sw.Close();
                s.Close();
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        //get location of bicycle
        static void LocationsAPI()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://kox947ka1a.execute-api.ap-northeast-2.amazonaws.com/prod/users/locations");

            request.Method = "GET";
            request.ContentType = "application/json";
            request.Headers.Add("Authorization", auth_key);

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader respStream = new StreamReader(response.GetResponseStream());
                string resp = respStream.ReadToEnd();

                respStream.Close();

                JObject ret = JObject.Parse(resp);

                pickUp.Clear();
                putDown.Clear();

                foreach (var element in ret["locations"])
                {
                    //Console.WriteLine($"id : {element["id"]}, located_bikes_count: {element["located_bikes_count"]}");

                    int y = mapSize - 1- Int32.Parse(element["id"].ToString()) % mapSize;
                    int x = Int32.Parse(element["id"].ToString()) / mapSize;
                    map[y, x] = Int32.Parse(element["located_bikes_count"].ToString());

                    if(map[y,x] > threshold)
                    {
                        pickUp.Add(new Block(x * mapSize + (mapSize - 1 - y), map[y, x]));
                    }
                    else if(map[y,x] < threshold)
                    {
                        putDown.Add(new Block(x * mapSize + (mapSize - 1 - y), map[y, x]));
                    }
                }

                pickUp.Sort(new descendingComp());
                putDown.Sort(new ascendingComp());

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static void LocationsAPI2(int t)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://kox947ka1a.execute-api.ap-northeast-2.amazonaws.com/prod/users/locations");

            request.Method = "GET";
            request.ContentType = "application/json";
            request.Headers.Add("Authorization", auth_key);

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader respStream = new StreamReader(response.GetResponseStream());
                string resp = respStream.ReadToEnd();

                respStream.Close();

                JObject ret = JObject.Parse(resp);

                pickUp.Clear();
                putDown.Clear();

                foreach (var element in ret["locations"])
                {
                    //Console.WriteLine($"id : {element["id"]}, located_bikes_count: {element["located_bikes_count"]}");

                    int y = mapSize - 1 - Int32.Parse(element["id"].ToString()) % mapSize;
                    int x = Int32.Parse(element["id"].ToString()) / mapSize;
                    map[y, x] = Int32.Parse(element["located_bikes_count"].ToString());

                    if (map[y, x] > thresholdAvg[t/240, y, x])
                    {
                        pickUp.Add(new Block(x * mapSize + (mapSize - 1 - y), map[y, x]));
                    }
                    else if (map[y, x] < thresholdAvg[t/240, y, x])
                    {
                        putDown.Add(new Block(x * mapSize + (mapSize - 1 - y), map[y, x]));
                    }
                }

                pickUp.Sort(new descendingComp());
                putDown.Sort(new ascendingComp());

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }


        //get information of trucks
        static void TrucksAPI()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://kox947ka1a.execute-api.ap-northeast-2.amazonaws.com/prod/users/trucks");

            request.Method = "GET";
            request.ContentType = "application/json";
            request.Headers.Add("Authorization", auth_key);

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader respStream = new StreamReader(response.GetResponseStream());
                string resp = respStream.ReadToEnd();

                respStream.Close();

                JObject ret = JObject.Parse(resp);

                foreach (var element in ret["trucks"])
                {
                    //Console.WriteLine($"id : {element["id"]}, location_id: {element["location_id"]}, loaded_bikes_count : {element["loaded_bikes_count"]}");

                    int id = Int32.Parse(element["id"].ToString());
                    int location_id = Int32.Parse(element["location_id"].ToString());
                    int loaded_bikes_count = Int32.Parse(element["loaded_bikes_count"].ToString());

                    truck[id] = new Truck(id, location_id, loaded_bikes_count);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        //do sim
        static void SimulateAPI(JArray obj = null)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://kox947ka1a.execute-api.ap-northeast-2.amazonaws.com/prod/users/simulate");

            request.Method = "PUT";
            request.ContentType = "application/json";
            request.Headers.Add("Authorization", auth_key);
            
            //jarray

            JObject item = new JObject();
            if (obj == null)
            {
                JArray element = new JArray();
                for (int i=0; i< truckNum; ++i)
                {
                    
                    JObject ele = new JObject();
                    JArray t = new JArray();
                    t.Add(0);

                    ele.Add("truck_id", i);
                    ele.Add("command", t);

                    element.Add(ele);
                }
                item.Add("commands", element);
            }
            else
            {

                item.Add("commands", obj);
            }

            //end

            StreamWriter reqStream = new StreamWriter(request.GetRequestStream());
            reqStream.Write(item);
            reqStream.Close();


            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader respStream = new StreamReader(response.GetResponseStream());
                string resp = respStream.ReadToEnd();
                respStream.Close();

                JObject ret = JObject.Parse(resp);
                
                Console.WriteLine("\n----Simulation Result----");
                Console.WriteLine($"status : {ret["status"]}");
                Console.WriteLine($"time : {ret["time"]}");
                Console.WriteLine($"failed_requests_count : {ret["failed_requests_count"]}");
                Console.WriteLine($"distance : {ret["distance"]}");
                Console.WriteLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }


        static void ScoreAPI()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://kox947ka1a.execute-api.ap-northeast-2.amazonaws.com/prod/users/score");

            request.Method = "GET";
            request.ContentType = "application/json";
            request.Headers.Add("Authorization", auth_key);

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader respStream = new StreamReader(response.GetResponseStream());
                string resp = respStream.ReadToEnd();

                respStream.Close();

                JObject ret = JObject.Parse(resp);

                Console.WriteLine($"score : {ret["score"]}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        #endregion


        #region properties

        static int mapSize = 5; // 5 60
        static int truckNum = 5;// 5 10
        static int threshold = 2;// 2 x
        static int[,,] threshold_ = new int[720, mapSize, mapSize];
        static double[,,] thresholdAvg = new double[3, mapSize, mapSize];
        static int truckLimit = 20;

        static int[,] map = new int[mapSize, mapSize];

        static Truck[] truck = new Truck[truckNum];

        static List<Block> pickUp = new List<Block>();
        static List<Block> putDown = new List<Block>();


        static string auth_key = "ebffe725-8184-4cc3-8b1b-3d2c1ef3a0b7";
        #endregion

        #region Methods
        //COORD -> ID  : x * mapSize + (mapSize - 1 - y)
        //ID -> COORD : x : id / mapSize ,    y : mapSize - 1 - (ID % mapSize)

        static void getPastData()
        {
            string[] FileNames = { "p2_day-1.json", "p2_day-2.json", "p2_day-3.json" };

            foreach (var FileName in FileNames)
            {
                FileStream fs = new FileStream(FileName, FileMode.Open);
                StreamReader sr = new StreamReader(fs);

                JObject readDataJson = new JObject();
                string readDataString = sr.ReadToEnd();

                readDataJson = JObject.Parse(readDataString);

                foreach (var day in readDataJson)
                {
                    // day.key : day
                    // day.value : requests

                    foreach (var req in day.Value)
                    {
                        // [0] : to borrow
                        // [1] : to return
                        // [2] : duration

                        int time = int.Parse(day.Key);
                        int borrowID = int.Parse(req[0].ToString());

                        int borrowX = borrowID / mapSize;
                        int borrowY = mapSize - 1 - (borrowID % mapSize);

                        ++threshold_[time, borrowY, borrowX];
                    }
                }

                for(int t=0; t<720; ++t)
                    for(int i=0; i<mapSize; ++i)
                        for(int j=0; j<mapSize; ++j)
                            thresholdAvg[t / 240, i, j] += threshold_[t, i, j];

                for (int t = 0; t < 3; ++t)
                    for (int i = 0; i < mapSize; ++i)
                        for (int j = 0; j < mapSize; ++j)
                        {
                            //thresholdAvg[t, i, j] /= 240;
                            thresholdAvg[t, i, j] /= 3;
                            thresholdAvg[t, i, j] = Math.Ceiling(thresholdAvg[t, i, j]);
                        }
                        

                sr.Close();
                fs.Close();
            }
        }

        #endregion


        static void Solution1()
        {
            LocationsAPI();
            TrucksAPI();

            JArray element = new JArray();

            for(int t = 0; t < truckNum; ++t)
            {
                int candiPickUpID = -1;   //where
                int candiPutDownID = -1;
                int candiBicycle = 0;   //how many
                int candidist = -1; //how long

                int candiPickUpIdx = -1;
                int candiPutDownIdx = -1;

                int truckX = truck[t].locationID / mapSize;
                int truckY = mapSize - 1 - truck[t].locationID % mapSize;

                for (int i = 0; i < pickUp.Count; ++i)
                {
                    for(int j = 0; j < putDown.Count; ++j)
                    {
                        int pickUpX = pickUp[i].ID / mapSize;
                        int pickUpY = mapSize - 1 - pickUp[i].ID % mapSize;

                        int putDownX = putDown[j].ID / mapSize;
                        int putDownY = mapSize - 1 - putDown[j].ID % mapSize;

                        int firstDist = Math.Abs(truckX - pickUpX) + Math.Abs(truckY - pickUpY);
                        int secondDist = Math.Abs(pickUpX - putDownX) + Math.Abs(pickUpY - putDownY);
                        int dist = firstDist + secondDist;

                        int canMoveBicycle = (10 - dist) / 2; //pick up and put down
                        canMoveBicycle = Math.Min(canMoveBicycle, Math.Min(pickUp[i].bicycle - threshold, threshold - putDown[j].bicycle)); // bicycle - threshold

                        if (candiBicycle < canMoveBicycle)
                        {
                            candiBicycle = canMoveBicycle;
                            candiPickUpID = pickUp[i].ID;
                            candiPutDownID = putDown[j].ID;
                            candiPickUpIdx = i;
                            candiPutDownIdx = j;
                            candidist = dist;
                        }
                        else if (candiBicycle == canMoveBicycle)
                        {
                            if (candidist > dist)
                            {
                                candiBicycle = canMoveBicycle;
                                candiPickUpID = pickUp[i].ID;
                                candiPutDownID = putDown[j].ID;
                                candiPickUpIdx = i;
                                candiPutDownIdx = j;
                                candidist = dist;
                            }
                        }
                    }
                }

                if(candiPickUpID != -1)
                {
                    JObject truckToDo = new JObject();
                    JArray truckDoList = new JArray();

                    int pickUpX = candiPickUpID / mapSize;
                    int pickUpY = mapSize - 1 - candiPickUpID % mapSize;

                    int putDownX = candiPutDownID / mapSize;
                    int putDownY = mapSize - 1 - candiPutDownID % mapSize;

                    // truck goes to pick up
                    if(truckX < pickUpX)
                    {
                        // go right
                        for (int i = 0; i < pickUpX - truckX; ++i)
                            truckDoList.Add(2);
                    }
                    else
                    {
                        // go left
                        for (int i = 0; i < truckX - pickUpX; ++i)
                            truckDoList.Add(4);
                    }

                    if(truckY < pickUpY)
                    {
                        // go down
                        for (int i = 0; i < pickUpY - truckY; ++i)
                            truckDoList.Add(3);
                    }
                    else
                    {
                        // go up
                        for (int i = 0; i < truckY - pickUpY; ++i)
                            truckDoList.Add(1);
                    }



                    // load bicycles
                    for (int i = 0; i < candiBicycle; ++i)
                        truckDoList.Add(5);



                    // truck goes to put down
                    if (pickUpX < putDownX)
                    {
                        // go right
                        for (int i = 0; i < putDownX - pickUpX; ++i)
                            truckDoList.Add(2);
                    }
                    else
                    {
                        // go left
                        for (int i = 0; i < pickUpX - putDownX; ++i)
                            truckDoList.Add(4);
                    }

                    if (pickUpY < putDownY)
                    {
                        // go down
                        for (int i = 0; i < putDownY - pickUpY; ++i)
                            truckDoList.Add(3);
                    }
                    else
                    {
                        // go up
                        for (int i = 0; i < pickUpY - putDownY; ++i)
                            truckDoList.Add(1);
                    }

                    // unload bicycles
                    for (int i = 0; i < candiBicycle; ++i)
                        truckDoList.Add(6);


                    pickUp[candiPickUpIdx].bicycle -= candiBicycle;
                    putDown[candiPutDownIdx].bicycle += candiBicycle;

                    truckToDo.Add("truck_id", t);
                    truckToDo.Add("command", truckDoList);
                    element.Add(truckToDo);
                }
                else
                {
                    JObject nothingToDo = new JObject();
                    nothingToDo.Add("truck_id", t);
                    nothingToDo.Add("command", new JArray());
                    element.Add(nothingToDo);
                }
            }

            SimulateAPI(element);
        }

        static void Solution2(int time)
        {
            LocationsAPI2(time);
            TrucksAPI();

            JArray element = new JArray();

            for (int t = 0; t < truckNum; ++t)
            {
                int candiPickUpID = -1;   //where
                int candiPutDownID = -1;
                int candiBicycle = 0;   //how many
                int candidist = -1; //how long

                int candiPickUpIdx = -1;
                int candiPutDownIdx = -1;

                int truckX = truck[t].locationID / mapSize;
                int truckY = mapSize - 1 - truck[t].locationID % mapSize;

                for (int i = 0; i < pickUp.Count; ++i)
                {
                    for (int j = 0; j < putDown.Count; ++j)
                    {
                        int pickUpX = pickUp[i].ID / mapSize;
                        int pickUpY = mapSize - 1 - pickUp[i].ID % mapSize;

                        int putDownX = putDown[j].ID / mapSize;
                        int putDownY = mapSize - 1 - putDown[j].ID % mapSize;

                        int firstDist = Math.Abs(truckX - pickUpX) + Math.Abs(truckY - pickUpY);
                        int secondDist = Math.Abs(pickUpX - putDownX) + Math.Abs(pickUpY - putDownY);
                        int dist = firstDist + secondDist;

                        int canMoveBicycle = (10 - dist) / 2; //pick up and put down
                        canMoveBicycle = Math.Min(canMoveBicycle, Math.Min(pickUp[i].bicycle - (int)thresholdAvg[time / 240, pickUpY, pickUpX], (int)thresholdAvg[time / 240, putDownY, putDownX] - putDown[j].bicycle)); // bicycle - threshold

                        if (candiBicycle < canMoveBicycle)
                        {
                            candiBicycle = canMoveBicycle;
                            candiPickUpID = pickUp[i].ID;
                            candiPutDownID = putDown[j].ID;
                            candiPickUpIdx = i;
                            candiPutDownIdx = j;
                            candidist = dist;
                        }
                        else if (candiBicycle == canMoveBicycle)
                        {
                            if (candidist > dist)
                            {
                                candiBicycle = canMoveBicycle;
                                candiPickUpID = pickUp[i].ID;
                                candiPutDownID = putDown[j].ID;
                                candiPickUpIdx = i;
                                candiPutDownIdx = j;
                                candidist = dist;
                            }
                        }
                    }
                }

                if (candiPickUpID != -1)
                {
                    JObject truckToDo = new JObject();
                    JArray truckDoList = new JArray();

                    int pickUpX = candiPickUpID / mapSize;
                    int pickUpY = mapSize - 1 - candiPickUpID % mapSize;

                    int putDownX = candiPutDownID / mapSize;
                    int putDownY = mapSize - 1 - candiPutDownID % mapSize;

                    // truck goes to pick up
                    if (truckX < pickUpX)
                    {
                        // go right
                        for (int i = 0; i < pickUpX - truckX; ++i)
                            truckDoList.Add(2);
                    }
                    else
                    {
                        // go left
                        for (int i = 0; i < truckX - pickUpX; ++i)
                            truckDoList.Add(4);
                    }

                    if (truckY < pickUpY)
                    {
                        // go down
                        for (int i = 0; i < pickUpY - truckY; ++i)
                            truckDoList.Add(3);
                    }
                    else
                    {
                        // go up
                        for (int i = 0; i < truckY - pickUpY; ++i)
                            truckDoList.Add(1);
                    }



                    // load bicycles
                    for (int i = 0; i < candiBicycle; ++i)
                        truckDoList.Add(5);



                    // truck goes to put down
                    if (pickUpX < putDownX)
                    {
                        // go right
                        for (int i = 0; i < putDownX - pickUpX; ++i)
                            truckDoList.Add(2);
                    }
                    else
                    {
                        // go left
                        for (int i = 0; i < pickUpX - putDownX; ++i)
                            truckDoList.Add(4);
                    }

                    if (pickUpY < putDownY)
                    {
                        // go down
                        for (int i = 0; i < putDownY - pickUpY; ++i)
                            truckDoList.Add(3);
                    }
                    else
                    {
                        // go up
                        for (int i = 0; i < pickUpY - putDownY; ++i)
                            truckDoList.Add(1);
                    }

                    // unload bicycles
                    for (int i = 0; i < candiBicycle; ++i)
                        truckDoList.Add(6);




                    pickUp[candiPickUpIdx].bicycle -= candiBicycle;
                    putDown[candiPutDownIdx].bicycle += candiBicycle;

                    if((int)thresholdAvg[time / 240, putDownY, putDownX] - putDown[candiPutDownIdx].bicycle > 0)
                    {
                        while((int)thresholdAvg[time / 240, putDownY, putDownX] - putDown[candiPutDownIdx].bicycle > 0)
                        {
                            if (truck[t].loaded_bikes_count <= 0)
                                break;

                            truckDoList.Add(6);
                            --truck[t].loaded_bikes_count;
                        }
                    }

                    truckToDo.Add("truck_id", t);
                    truckToDo.Add("command", truckDoList);
                    element.Add(truckToDo);
                }
                else
                {
                    // go to pick up
                    JObject nothingToDo = new JObject();
                    JArray truckDoList = new JArray();

                    int gotoPickupID = -1;
                    int bicycleNum = 0;
                    int gotoPickupIdx = -1;

                    for (int i = 0; i < pickUp.Count; ++i)
                    {
                        int tempX = pickUp[i].ID / mapSize;
                        int tempY = mapSize - 1 - pickUp[i].ID % mapSize;

                        if (bicycleNum < pickUp[i].bicycle - (int)thresholdAvg[time/240, tempY, tempX])
                        {
                            bicycleNum = pickUp[i].bicycle - (int)thresholdAvg[time/240, tempY, tempX];
                            gotoPickupID = pickUp[i].ID;
                            gotoPickupIdx = i;
                        }
                    }


                    if (gotoPickupID == -1)
                    {
                        nothingToDo.Add("truck_id", t);
                        nothingToDo.Add("command", truckDoList);
                        element.Add(nothingToDo);
                    }
                    else
                    {
                        int pickUpX = gotoPickupID / mapSize;
                        int pickUpY = mapSize - 1 - gotoPickupID % mapSize;


                        // truck goes to pick up
                        if (truckX < pickUpX)
                        {
                            // go right
                            for (int i = 0; i < pickUpX - truckX; ++i)
                                truckDoList.Add(2);
                        }
                        else
                        {
                            // go left
                            for (int i = 0; i < truckX - pickUpX; ++i)
                                truckDoList.Add(4);
                        }

                        if (truckY < pickUpY)
                        {
                            // go down
                            for (int i = 0; i < pickUpY - truckY; ++i)
                                truckDoList.Add(3);
                        }
                        else
                        {
                            // go up
                            for (int i = 0; i < truckY - pickUpY; ++i)
                                truckDoList.Add(1);
                        }

                        int cnt = 0;

                        // load bicycles
                        for (int i = 0; i < bicycleNum; ++i)
                        {
                            if (truck[t].loaded_bikes_count + cnt < 20)
                            {
                                truckDoList.Add(5);
                                ++cnt;
                            }
                        }
                        pickUp[gotoPickupIdx].bicycle -= cnt;
                    }
                }
            }

            SimulateAPI(element);

        }


        static void Main(string[] args)
        {
            //StartAPI();

            // scenario 1
             LocationsAPI();
             TrucksAPI();
             SimulateAPI(); // first.. 


             for(int i=0; i<719; ++i)
                 Solution1();

             ScoreAPI();


            // scenario 2
            /* getPastData();
             LocationsAPI2(0);
             TrucksAPI();
             SimulateAPI();

             for (int i = 1; i < 720; ++i)
                 Solution2(i);
             ScoreAPI();
             */


        }
    }
}