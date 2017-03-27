using UnityEngine;
using System.Collections;
using System;
using System.IO;

namespace BattlefieldVisualisation
{
    public class MapTilesClient
    {
        private string tilesDirPath = "C:/Unity/MapTiles";
        private int serverIndex = 0;

        public MapTilesClient()
        {
            // create tiles directory if it doesn't exists
            if (!Directory.Exists(tilesDirPath))
            {
                Directory.CreateDirectory(tilesDirPath);
            }
        }

        public IEnumerator _RefreshWithCaching(GameObject mapTile, int x, int y, int zoom, char mapType)
        {
            int max = MaxCoordinateValueOnZoomLevel(zoom);
            if (x > max || y > max || x < 0 || y < 0)
            {
                yield break;
            }

            string dir = string.Format("{0}/{1}/z{2}/x{3}", tilesDirPath, mapType, zoom, x);
            string pathToCachedTile = string.Format("{0}/y{1}.png", dir, y);

            // try to load from cache
            if (File.Exists(pathToCachedTile))
            {               
                var ccc = new WWW("file:///" + pathToCachedTile);
                yield return ccc;
                mapTile.GetComponent<Renderer>().material.mainTexture = ccc.texture;
                mapTile.SetActive(true);
            }
            
            else
            // get from api
            {
                var url = string.Format("http://mt{0}.google.com/vt/lyrs={1}@901000000&hl=en&z={2}&x={3}&y={4}", serverIndex, mapType, zoom, x, y);
                serverIndex = (serverIndex + 1) % 4;
              
                var www = new WWW(url);
                yield return www;

                if (www.error != null)
                {
                    Debug.Log("error: " + www.error + " " + url);
                }
                else
                {
                    mapTile.GetComponent<Renderer>().material.mainTexture = www.texture;
                    mapTile.SetActive(true);

                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    // cache new tile
                    File.WriteAllBytes(pathToCachedTile, www.bytes);
                }
            }
        }

        public IEnumerator _Refresh(GameObject mapTile, int x, int y, int zoom, char mapType)
        {
            int max = MaxCoordinateValueOnZoomLevel(zoom);
            if (x > max || y > max || x < 0 || y < 0)
            {
                yield break;
            }

            var url = string.Format("http://mt{0}.google.com/vt/lyrs={1}@901000000&hl=en&z={2}&x={3}&y={4}", serverIndex, mapType, zoom, x, y);
            serverIndex = (serverIndex + 1) % 4;

            var www = new WWW(url);
            yield return www;

            if (www.error != null)
            {
                mapTile.SetActive(false);
                Debug.Log("error: " + www.error + " " + url);
            }
            else
            {
                mapTile.GetComponent<Renderer>().material.mainTexture = www.texture;
            }
        }       

        public static int NumberOfTiles(int zoomLevel)
        {
            return (int)Math.Pow(2, (2 * zoomLevel));
        }

        public static int MaxCoordinateValueOnZoomLevel(int zoomLevel)
        {
            return (int)Math.Pow(2, zoomLevel) - 1;
        }
    }

}
