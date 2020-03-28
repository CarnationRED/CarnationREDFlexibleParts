using System.Xml.Serialization;

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

    [XmlElement("radius_lower__left")] public float radius_0;
    [XmlElement("radius_lower_right")] public float radius_1;
    [XmlElement("radius_upper__left")] public float radius_3;
    [XmlElement("radius_upper_right")] public float radius_2;

    public void OnSerialize()
    {
        radius_0 = radius[0];
        radius_1 = radius[1];
        radius_3 = radius[3];
        radius_2 = radius[2];
    }
    public void OnDeserialize()
    {
        radius = new float[4];
        radius[0] = radius_0;
        radius[1] = radius_1;
        radius[3] = radius_3;
        radius[2] = radius_2;
        if (height < 0.001f) height = 0.001f;
        if (width < 0.001f) width = 0.001f;
    }
}