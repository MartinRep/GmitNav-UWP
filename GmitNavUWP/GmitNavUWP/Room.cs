using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmitNavUWP
{
    public class Room
    {
        private String name;
        private List<String> aliases = null;
        private Double lat, lng;
        private int level;

        public Room(String name)
        {
            this.name = name;
        }

        public String getName()
        {
            return name;
        }

        public void setName(String name)
        {
            this.name = name;
        }

        public List<String> getAliases()
        {
            return aliases;
        }

        public void setAliases(List<String> aliases)
        {
            this.aliases = aliases;
        }

        public void addAlias(String alias)
        {
            aliases.Add(alias);
        }

        public Double getLat()
        {
            return lat;
        }

        public void setLat(Double lat)
        {
            this.lat = lat;
        }

        public Double getLng()
        {
            return lng;
        }

        public void setLng(Double lng)
        {
            this.lng = lng;
        }

        public int getLevel()
        {
            return level;
        }

        public void setLevel(int level)
        {
            this.level = level;
        }
    }
}
