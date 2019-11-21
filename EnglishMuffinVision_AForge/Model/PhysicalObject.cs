using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABI.Robotics.Model
{
    class PhysicalObject
    {
        private String _Type;
        //private Size _Size;
        //private Location _Location;

        public String Type
        {
            get { return _Type; }
            set { _Type = value; }
        }

        //public Size Size
        //{
        //    get { return _Size; }
        //    set { _Size = value; }
        //}
        //public Location Location
        //{
        //    get { return _Location; }
        //    set { _Location = value; }
        //}

    }

    //internal struct Size
    //{
    //    public double Width;
    //    public double Length;
    //    public double Height;
    //}

    //internal struct Location
    //{
    //    public double X;
    //    public double Y;
    //    public double Z;
    //}
}
