using System.Xml.Serialization;

namespace CarnationVariableSectionPart.UI
{
    [XmlRoot("SectionInfo", IsNullable = true)]
    public class SectionInfo
    {
        [XmlAttribute("name")]
        public string name;
        [XmlElement("width")]
        public float width;
        [XmlElement("height")]
        public float height;
        [XmlIgnore]
        public float[] radius;
        [XmlIgnore]
        public string[] corners;

        [XmlElement("radius_lower__left")] public float radius_0;
        [XmlElement("radius_lower_right")] public float radius_1;
        [XmlElement("radius_upper__left")] public float radius_3;
        [XmlElement("radius_upper_right")] public float radius_2;

        [XmlElement("corner_type1")] public string corner_0;
        [XmlElement("corner_type2")] public string corner_1;
        [XmlElement("corner_type3")] public string corner_3;
        [XmlElement("corner_type4")] public string corner_2;

        public void OnSerialize()
        {
            radius_0 = radius[0];
            radius_1 = radius[1];
            radius_3 = radius[3];
            radius_2 = radius[2];
            corner_0 = corners[0];
            corner_1 = corners[1];
            corner_3 = corners[3];
            corner_2 = corners[2];
        }
        public void OnDeserialize()
        {
            radius = new float[4];
            radius[0] = radius_0;
            radius[1] = radius_1;
            radius[3] = radius_3;
            radius[2] = radius_2;
            corners = new string[4];
            corners[0] = corner_0;
            corners[1] = corner_1;
            corners[3] = corner_3;
            corners[2] = corner_2;


            if (height < 0.001f) height = 0.001f;
            if (width < 0.001f) width = 0.001f;
        }
    }
}