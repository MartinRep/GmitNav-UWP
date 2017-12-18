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
        List<Room> rooms = new List<Room>();
        Boolean GotRooms = false;
        TextBox searchBox;
        Geopoint gmit = new Geopoint(new BasicGeoposition()
        {
            Latitude = Util.Gmit.LAT,
            Longitude = Util.Gmit.LNG
        });
        public MainPage()
        {
            this.InitializeComponent();
            searchBox = FindName("RoomTextBox") as TextBox;
            gmitMap.Loaded += MapConfigAsync;
            button0.Click += new RoutedEventHandler((object sender, RoutedEventArgs e) => { ChangeLevel(0); }); 
            button1.Click += new RoutedEventHandler((object sender, RoutedEventArgs e) => { ChangeLevel(1); }); 
            button2.Click += new RoutedEventHandler((object sender, RoutedEventArgs e) => { ChangeLevel(2); });
            button3.Click += new RoutedEventHandler((object sender, RoutedEventArgs e) => { ChangeLevel(3); });
            searchBox.KeyDown += SearchBox_KeyDown;
            gmitMap.MapElementClick += GmitMap_MapElementClick;
            Task.Run( () => GetRooms());
        }

        private void SearchBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                SearchButton_ClickAsync(sender, e);
            }
            else if(e.Key == VirtualKey.Escape)
            {
                searchBox.Text = "";
            }
        }

        private void GmitMap_MapElementClick(MapControl sender, MapElementClickEventArgs args)
        {
            Debug.WriteLine("Lat: " + args.Location.Position.Latitude +" Long:"+args.Location.Position.Longitude);
        }

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
                    var theData = JsonObject.Parse(response);
                    JsonArray data = theData.GetNamedArray("results").GetObjectAt(0).GetNamedArray("data");
                    for (int i = 0; i < data.Count; i++)
                    {
                        JsonArray nodeJSON = data.GetObjectAt(Convert.ToUInt32(i)).GetNamedArray("row");
                        JsonObject node = nodeJSON.GetObjectAt(0);
                        rooms.Add(JsonConvert.DeserializeObject<Room>(node.ToString()));
                    }
                    GotRooms = true;
                    Debug.WriteLine(rooms.Count.ToString());
                }
            }
            catch(Exception)
            {
                ShowToastNotification("Network Error", "There seems to a problem conneting to the server.");
            }
        }

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
        public async Task<int> ShowRoomAsync(Room room)
        {
            int index = -1;
            if (!GotRooms)
            {
                ShowToastNotification("Waiting for Response", "Database poll was not succesfull or still in progress");
            }
            else
            {
                index = AddMarker(room.lat, room.lng, room.name);
                ChangeLevel(room.level);
                searchBox.Text = "";
                await gmitMap.TrySetViewAsync(new Geopoint(new BasicGeoposition()
                { Latitude = room.lat, Longitude = room.lng }), 19D, 0, 0);

            }
            return index;
        }

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

        private void MapZoomControl(MapControl sender, object args)
        {
            Debug.WriteLine(sender.ZoomLevel);
            if (sender.ZoomLevel < Util.MIN_ZOOM) sender.ZoomLevel = Util.MIN_ZOOM;
            else if (sender.ZoomLevel > Util.MAX_ZOOM) sender.ZoomLevel = Util.MAX_ZOOM;
        }

        public async void MapConfigAsync(object sender, RoutedEventArgs e)
        {
            gmitMap.LandmarksVisible = false;
            gmitMap.Visibility = Visibility.Visible;
            await AddMapOverlayAsync(Util.Building.Old.NORTH, Util.Building.Old.WEST, new Uri("ms-appx:///Assets/GmitMaps/dgmit0.png")); //GroundLevel
            await AddMapOverlayAsync(Util.Building.Old.NORTH, Util.Building.Old.WEST, new Uri("ms-appx:///Assets/GmitMaps/dgmit1.png")); // First Level
            await AddMapOverlayAsync(Util.Building.Old.NORTH, Util.Building.Old.WEST, new Uri("ms-appx:///Assets/GmitMaps/dgmit2.png")); // Second Level
            await AddMapOverlayAsync(Util.Building.Old.NORTH, Util.Building.Old.WEST, new Uri("ms-appx:///Assets/GmitMaps/dgmit3.png")); //SubLevel
            ChangeLevel(0); //Set initial level to Ground Level
            Geopoint buttonsGeo = new Geopoint(new BasicGeoposition()
            {
                Latitude = Util.Buttons.LAT,
                Longitude = Util.Buttons.LNG
            });
            MapControl.SetLocation(Levels, buttonsGeo);
            MapControl.SetNormalizedAnchorPoint(Levels, new Point(0D,0D));
            Levels.Visibility = Visibility.Visible;
            await gmitMap.TrySetViewAsync(gmit, 19D, 0, 0);
            gmitMap.ZoomLevelChanged += MapZoomControl;
            SearchButton.Click += SearchButton_ClickAsync;
            //gmitMap.CenterChanged += CenterBoundries;  //GPS locations calibration needed
        }

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
                    try
                    {
                        var count = gmitMap.Children.Count;
                        if (gmitMap.Layers.Count > 4)
                        {
                            gmitMap.Layers.ElementAt(gmitMap.Layers.Count - 1).Visible = false;
                            gmitMap.Layers.RemoveAt(gmitMap.Layers.Count -1);   //Previous Room icon erase
                        }
                    }
                    catch(Exception exep)
                    {
                        Debug.WriteLine(exep.Message);
                    }
                    await ShowRoomAsync(destRoom);
                }
                else
                {
                    ShowToastNotification("Room Name", "Room number / Alias doesn't exist in the Database");
                }
            }
        }


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
            gmitMap.Layers.Add(LandmarksPhotoLayer);

            return index;
        }

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

        private void ChangeLevel(int level)
        {
            List<Button> buttons = new List<Button>();
            buttons.Add(button0);
            buttons.Add(button1);
            buttons.Add(button2);
            buttons.Add(button3);
            for (int i = 0; i <= 3; i++)
            {
                if (i == level)
                {
                    gmitMap.Layers.ElementAt(i).Visible = true;
                    buttons.ElementAt(i).IsEnabled = false;
                }
                else
                {
                    gmitMap.Layers.ElementAt(i).Visible = false;
                    buttons.ElementAt(i).IsEnabled = true;
                }
            }
        }
    }
}


