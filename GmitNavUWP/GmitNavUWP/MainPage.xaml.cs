using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Newtonsoft.Json;
using GmitNavUWP.Service;
using Windows.Data.Json;
using Windows.UI.Notifications;
using Windows.UI.Xaml.Input;
using Windows.System;

namespace GmitNavUWP
{
    
    public sealed partial class MainPage : Page
    {
        List<Room> rooms = new List<Room>();    //List of all GMIT Rooms. Populated via GetRooms() 
        Boolean GotRooms = false;   //If data retreived from server are succesfull and List populated becames True
        TextBox searchBox;
        int searchedRoomLevel = 255;    //Last room number searched, used in ChangeLevel()
        Geopoint gmit = new Geopoint(new BasicGeoposition() //center of GMIT location 
        {
            Latitude = Util.Gmit.LAT,
            Longitude = Util.Gmit.LNG
        });
        public MainPage()
        {
            this.InitializeComponent();
            searchBox = FindName("RoomTextBox") as TextBox;
            gmitMap.Loaded += MapConfigAsync;   //Listener for Map loaded.
            button0.Click += new RoutedEventHandler((object sender, RoutedEventArgs e) => { ChangeLevel(0); }); //Lesteners for change Level Buttons
            button1.Click += new RoutedEventHandler((object sender, RoutedEventArgs e) => { ChangeLevel(1); }); 
            button2.Click += new RoutedEventHandler((object sender, RoutedEventArgs e) => { ChangeLevel(2); });
            button3.Click += new RoutedEventHandler((object sender, RoutedEventArgs e) => { ChangeLevel(3); });
            searchBox.KeyDown += SearchBox_KeyDown; // Listener for Enter and Escape buttons in searchBox
            gmitMap.MapElementClick += GmitMap_MapElementClick; //For calibration purposes, to get Locations for maps and buttons
            Task.Run( () => GetRooms());    //Runs in background Rooms details retreive from server
        }

        // Listener for Enter and Escape buttons in searchBox
        private void SearchBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                SearchButton_ClickAsync(sender, e);
            }
            else if(e.Key == VirtualKey.Escape)
            {
                searchBox.Text = "";
                if(gmitMap.Layers.Count > 4)    //Checks if there is an Icon already and removes it.
                {
                    gmitMap.Layers.ElementAt(4).Visible = false;
                    gmitMap.Layers.RemoveAt(4);
                }

            }
        }

        //Dev only
        private void GmitMap_MapElementClick(MapControl sender, MapElementClickEventArgs args)
        {
            Debug.WriteLine("Lat: " + args.Location.Position.Latitude +" Long:"+args.Location.Position.Longitude);
        }

        // Gets Neo4jDb to retreive via HttpClient and REST architecture Rooms from server
        public async void GetRooms()
        {
            Neo4jDb db = new Neo4jDb();
            try
            {
                String response = await db.CypherAsync("MATCH (r:Room) RETURN r", "");
                if (response == null)
                {
                    ShowToastNotification("Network Error", "There seems to a problem conneting to the server.");
                }
                else
                {
                    /* construct a Json Request from Neo4j Database. Json format:
                     * {"statements": 
                        	[{"statement":"match (r) return r;", "params":{}}]
                        }
                     * 
                     */
                    var theData = JsonObject.Parse(response);
                    JsonArray data = theData.GetNamedArray("results").GetObjectAt(0).GetNamedArray("data");
                    for (int i = 0; i < data.Count; i++)
                    {
                        JsonArray nodeJSON = data.GetObjectAt(Convert.ToUInt32(i)).GetNamedArray("row");
                        JsonObject node = nodeJSON.GetObjectAt(0);
                        rooms.Add(JsonConvert.DeserializeObject<Room>(node.ToString()));
                    }
                    GotRooms = true;                   }
            }
            catch(Exception)
            {
                ShowToastNotification("Network Error", "There seems to a problem conneting to the server.");
            }
        }

        // Look for the Room with name in Rooms array
        public Room FindRoom(String name)
        {
            foreach (Room room in rooms)
            {
                if (room.name == name)
                {
                    return room;
                }
            }
            return null;
        }

        // Takes in the Room and display the Icon on map
        public async Task<int> ShowRoomAsync(Room room)
        {
            int index = -1;
            if (!GotRooms)
            {
                ShowToastNotification("Waiting for Response", "Database poll was not succesfull or still in progress");
            }
            else
            {
                index = AddMarker(room.lat, room.lng, room.name);   //Call to create and display the marker icon
                searchedRoomLevel = room.level; // For change of Level purposes. Icon only appears on the corrent level
                ChangeLevel(room.level);    
                searchBox.Text = "";
                await gmitMap.TrySetViewAsync(new Geopoint(new BasicGeoposition()
                { Latitude = room.lat, Longitude = room.lng }), 19D, 0, 0);         //Asych method to change view inside Bing maps
            }
            return index;
        }

        // EventHandler to keep user inside GMIT boundiers. Causing bit trouble at moment. Dissabled for now.
        private void CenterBoundries(MapControl sender, object args)
        {
            Debug.WriteLine(gmitMap.Center.Position.Latitude);
            Debug.WriteLine(gmitMap.Center.Position.Longitude);
            var gmit = new Geopoint(new BasicGeoposition()
            {
                Latitude = Util.Building.Old.NORTH,
                Longitude = Util.Building.Old.WEST
            });
            if (gmitMap.Center.Position.Latitude > Util.CameraBoundries.WEST
                || gmitMap.Center.Position.Latitude < Util.CameraBoundries.EAST
                || gmitMap.Center.Position.Longitude > Util.CameraBoundries.NORTH
                || gmitMap.Center.Position.Latitude < Util.CameraBoundries.SOUTH)
                gmitMap.Center = gmit; //await gmitMap.TrySetViewAsync(gmit, 19D, 0, 0);
        }

        // EventHandler for Map Zoom level. Restricting users Zoom level
        private void MapZoomControl(MapControl sender, object args)
        {
            Debug.WriteLine(sender.ZoomLevel);
            if (sender.ZoomLevel < Util.MIN_ZOOM) sender.ZoomLevel = Util.MIN_ZOOM;
            else if (sender.ZoomLevel > Util.MAX_ZOOM) sender.ZoomLevel = Util.MAX_ZOOM;
        }


        // Initial Map configuration. All the Map overlays are loaded and placed in right GPS coordinates
        public async void MapConfigAsync(object sender, RoutedEventArgs e)
        {
            gmitMap.LandmarksVisible = false;
            await AddMapOverlayAsync(Util.Building.Old.NORTH, Util.Building.Old.WEST, new Uri("ms-appx:///Assets/GmitMaps/dgmit0.png")); //GroundLevel
            await AddMapOverlayAsync(Util.Building.Old.NORTH, Util.Building.Old.WEST, new Uri("ms-appx:///Assets/GmitMaps/dgmit1.png")); // First Level
            await AddMapOverlayAsync(Util.Building.Old.NORTH, Util.Building.Old.WEST, new Uri("ms-appx:///Assets/GmitMaps/dgmit2.png")); // Second Level
            await AddMapOverlayAsync(Util.Building.Old.NORTH, Util.Building.Old.WEST, new Uri("ms-appx:///Assets/GmitMaps/dgmit3.png")); //SubLevel
            ChangeLevel(0); //Set initial level to Ground Level
            //Location of Change Level Buttons on Map
            Geopoint buttonsGeo = new Geopoint(new BasicGeoposition()
            {
                Latitude = Util.Buttons.LAT,
                Longitude = Util.Buttons.LNG
            });
            MapControl.SetLocation(Levels, buttonsGeo); //Setting the location for buttons StackPanel
            MapControl.SetNormalizedAnchorPoint(Levels, new Point(0D,0D));
            Levels.Visibility = Visibility.Visible;     //Making them visible
            await gmitMap.TrySetViewAsync(gmit, 19D, 0, 0);
            gmitMap.ZoomLevelChanged += MapZoomControl; // Registering EventListener for Map change of Zoom
            SearchButton.Click += SearchButton_ClickAsync;  // Registering EventListener for Search Button
            LoadingIndicator.IsActive = false;      // App launch Loading Indicator (Left top corner) being disalbed after map is configured/
            SearchPanel.Visibility = Visibility.Visible;    //Search Panel with TextBox and Search Button
            searchBox.Focus(FocusState.Pointer);    // TextBox Autofocus
            // Currently disabled
            //gmitMap.CenterChanged += CenterBoundries;  //GPS locations calibration needed
        }


        // EventHandler for Search Button
        private async void SearchButton_ClickAsync(object sender, RoutedEventArgs e)
        {
           if(!GotRooms)
            {
                ShowToastNotification("Rooms Database", "Error accessing Rooms Database. Polling in progress. Wait and Try again...");
            }else
            {
                Room destRoom = FindRoom(searchBox.Text);
                if (destRoom != null)
                {
                    var count = gmitMap.Layers.Count;
                    // If there is a previously searched romm icon already on the map, hide and delete it.
                    if (count > 4)
                    {
                        gmitMap.Layers.ElementAt(count - 1).Visible = false;
                        gmitMap.Layers.RemoveAt(count -1);   //Previous Room icon erase
                        searchedRoomLevel = 255;
                    } else  // First time room search
                    {
                        searchedRoomLevel = destRoom.level;
                    }
                    await ShowRoomAsync(destRoom);
                }
                else
                {
                    ShowToastNotification("Room Name", "Room number / Alias doesn't exist in the Database");
                }
            }
        }

        // Funtion to add Image overlay to MapControl. Zoom level is very important when inserting picture overlay, as it determines the size and location of overlay.
        // It corespond with actual view on the map. The change view method of Bing maps is Async, making it difficult to place image correctly.
        public async Task<int> AddMapOverlayAsync(Double latitude, Double longtitude, Uri img)
        {
            // Defining position for image overlay. Map View ZoomLevel defines the images size relative to map
            Geopoint position = new Geopoint(new BasicGeoposition()
            {
                Latitude = latitude,
                Longitude = longtitude,
                Altitude = 0
            }, AltitudeReferenceSystem.Surface);
            //Making sure the view ZoomLevel is correct for inserting image as overlay.
            while (gmitMap.ZoomLevel < 18.9)
            {
                await gmitMap.TrySetViewAsync(gmit, 19D, 0, 0);
                await Task.Delay(1000);
                Debug.WriteLine(gmitMap.ZoomLevel);
            }
            // Create MapBillboard Image.
            RandomAccessStreamReference imgStream =
                RandomAccessStreamReference.CreateFromUri(img);
            var mapBillboard = new MapBillboard(gmitMap.ActualCamera)
            {
                Location = position,
                NormalizedAnchorPoint = new Point(0D, 0D),
                Image = imgStream,
            };
            int index = gmitMap.Layers.Count;
            var GmitFloorMaps = new List<MapElement>
            {
                mapBillboard
            };
            var LandmarksPhotoLayer = new MapElementsLayer
            {
                ZIndex = 0,
                MapElements = GmitFloorMaps,
                Visible = false
            };
            // Image is added as an new Layer to MapControl. ZIndex must be 0, so Icon with Zindex 1 will be on top of overlay.
            gmitMap.Layers.Add(LandmarksPhotoLayer);
            return index;
        }

        // Method for adding Room Location Icon
        public int AddMarker(Double latitude, Double longtitude, String title)
        {
            int index = gmitMap.Layers.Count;
            var MyLandmarks = new List<MapElement>();
            Geopoint snPoint = new Geopoint(new BasicGeoposition()
            {
                Latitude = latitude,
                Longitude = longtitude,
                Altitude = 0
            }, AltitudeReferenceSystem.Surface);
            var spaceNeedleIcon = new MapIcon
            {
                Location = snPoint,
                NormalizedAnchorPoint = new Point(0D, 0D),
                ZIndex = 1,
                Title = title
            };
            MyLandmarks.Add(spaceNeedleIcon);
            var LandmarksLayer = new MapElementsLayer
            {
                ZIndex = 1,
                MapElements = MyLandmarks
            };
            gmitMap.Layers.Add(LandmarksLayer);
            return index;
        }

        // Method to display Toast messages, mostly Errors, with sound and 4 seconds duration.
        private void ShowToastNotification(string title, string stringContent)
        {
            ToastNotifier ToastNotifier = ToastNotificationManager.CreateToastNotifier();
            Windows.Data.Xml.Dom.XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
            Windows.Data.Xml.Dom.XmlNodeList toastNodeList = toastXml.GetElementsByTagName("text");
            toastNodeList.Item(0).AppendChild(toastXml.CreateTextNode(title));
            toastNodeList.Item(1).AppendChild(toastXml.CreateTextNode(stringContent));
            Windows.Data.Xml.Dom.IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
            Windows.Data.Xml.Dom.XmlElement audio = toastXml.CreateElement("audio");
            audio.SetAttribute("src", "ms-winsoundevent:Notification.SMS");
            ToastNotification toast = new ToastNotification(toastXml);
            toast.ExpirationTime = DateTime.Now.AddSeconds(4);
            ToastNotifier.Show(toast);
        }

        // Method to change Level of Map overlays. Ensuring the Room icon is only displayed with right Level.
        private void ChangeLevel(int level)
        {
            List<Button> buttons = new List<Button>();
            buttons.Add(button0);
            buttons.Add(button1);
            buttons.Add(button2);
            buttons.Add(button3);
            // Disabling Button of currently selected Level
            for (int i = 0; i <= 3; i++)
            {
                if (i == level)
                {
                    gmitMap.Layers.ElementAt(i).Visible = true;     // Showing Map overlay
                    buttons.ElementAt(i).IsEnabled = false;         //Disabling current level button
                }
                else
                {
                    gmitMap.Layers.ElementAt(i).Visible = false;     //Hiding rest of the map Overlays
                    buttons.ElementAt(i).IsEnabled = true;          //Enabling the rest of the level buttons
                }
            }
            if (gmitMap.Layers.Count > 4)       //if previous searched room icon is being displayed
            {
                if (searchedRoomLevel != level)     //Checking the searched room level is of Level currently displaying, if not hides Icon
                {
                    gmitMap.Layers.ElementAt(4).Visible = false;
                }
                else
                {
                    gmitMap.Layers.ElementAt(4).Visible = true;
                }
            }
        }
    }
}


