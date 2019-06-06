using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;
using UnityEngine;

public class osm_connection_test : MonoBehaviour
{
    private const string overpassApi = "https://www.openstreetmap.org/api/0.6/map?bbox=";
    public float bLeft = 20.44509f;
    public float bDown = 52.05174f;
    public float bRight = 20.44930f;
    public float bUp = 52.05320f;

    public bool bUseMockup = false;
    public string mockupPath = "D:\\map";



    private float minlat, minlon, maxlat, maxlon;

    private struct WayStruct
    {
        public long id;
        public NodeStruct[] nodes;
        public NodeStructTags[] tags;
    }

    private WayStruct[] WayStructs;


    private struct NodeStruct
    {
        public Vector2 position;
        public long id;
        public NodeStructTags[] structTags;

        public bool bContainsTags => structTags.Length > 0;
    }

    private struct NodeStructTags
    {
        public string key;
        public string value;
    }

    private NodeStruct[] nodeStructs;

    void Start()
    {
        byte[] osmStream = null;
        if (!bUseMockup)
        {
            string urlProvider = $"{overpassApi}{getFloatPoint(bLeft)},{getFloatPoint(bDown)},{getFloatPoint(bRight)},{getFloatPoint(bUp)}";
            WebClient wc = new WebClient();
            byte[] webStream = wc.DownloadData(urlProvider);
            wc.Dispose();
            if(webStream.Length > 0)
            {
                osmStream = webStream;
                File.WriteAllBytes("D:/test.osm", osmStream);
            }
        }
        else
            osmStream = File.ReadAllBytes(mockupPath);
        string xmlMagic = System.Text.Encoding.UTF8.GetString(osmStream, 0, 5);
        if(xmlMagic != "<?xml")
        {
            Debug.LogError("osm does NOT start with <?xml");
            return;
        }
        XmlDocument xml = new XmlDocument();
        xml.LoadXml(System.Text.Encoding.UTF8.GetString(osmStream));
        XmlNode bound = xml.GetElementsByTagName("bounds").Item(0);
        minlat = float.Parse(bound.Attributes.GetNamedItem("minlat").Value.Replace('.', ','));
        minlon = float.Parse(bound.Attributes.GetNamedItem("minlon").Value.Replace('.', ','));
        maxlat = float.Parse(bound.Attributes.GetNamedItem("maxlat").Value.Replace('.', ','));
        maxlon = float.Parse(bound.Attributes.GetNamedItem("maxlon").Value.Replace('.', ','));

        XmlNodeList nodes = xml.GetElementsByTagName("node");
        XmlNodeList ways = xml.GetElementsByTagName("way");
        XmlNodeList relations = xml.GetElementsByTagName("relation");
        ParseNodes(nodes);
        ParseWays(ways);


        CreateBuildings();
    }

    private void CreateBuildings()
    {
        foreach(var str in WayStructs)
        {
            //if (str.id != 196576473)
            //    continue;
            

            GameObject go = new GameObject(str.id.ToString());

            go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();

            var mf = go.GetComponent<MeshFilter>();
            Mesh mesh = new Mesh();
            mf.mesh = mesh;


            Vector2[] points = str.nodes.Select(x => x.position).ToArray();
            points = points.Take(points.Length - 1).ToArray();
            Vector2 scaleVec = new Vector2(10000, 10000);
            for (int n = 0; n < points.Length; n++)
            {
                points[n] = new Vector2((points[n].x - minlat)*-1f, points[n].y-minlon);
                points[n].Scale(scaleVec);
            }
            Triangulator tr = new Triangulator(points);
            int[] indices = tr.Triangulate();
            Vector3[] vec3d = new Vector3[points.Length];
            for (int i = 0; i < vec3d.Length; i++)
                vec3d[i] = new Vector3(points[i].x, 0,points[i].y);

            mesh.vertices = vec3d;
            mesh.triangles = indices;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            

            //mesh.vertices = new Vector3[str.nodes.Length];
        }
    }

    private void ParseWays(XmlNodeList ways)
    {
        List<WayStruct> ws = new List<WayStruct>();
        foreach (XmlNode node in ways)
        {
            long id = long.Parse(node.Attributes.GetNamedItem("id").Value);
            List<NodeStruct> ns = new List<NodeStruct>();
            List<NodeStructTags> nst = new List<NodeStructTags>();
            foreach (XmlNode child in node.ChildNodes)
                if (child.Name == "nd")
                    ns.Add(nodeStructs.Where(x => x.id == long.Parse(child.Attributes.GetNamedItem("ref").Value)).First());
                else if (child.Name == "tag")
                    nst.Add(GetStructTagTag(child));
            ws.Add(new WayStruct() { nodes = ns.ToArray(), id = id, tags = nst.ToArray() });
        }
        WayStructs = ws.ToArray();
    }

    private NodeStructTags GetStructTagTag(XmlNode child)
    {
        string key = child.Attributes.GetNamedItem("k").Value;
        string getter = child.Attributes.GetNamedItem("v").Value;
        return new NodeStructTags() { key = key, value = getter };
    }

    private void ParseNodes(XmlNodeList nodes)
    {
        List<NodeStruct> lns = new List<NodeStruct>();
        foreach(XmlNode node in nodes)
        {
            Vector2 pos = new Vector2(
                float.Parse(node.Attributes.GetNamedItem("lat").Value.Replace('.', ',')),
                float.Parse(node.Attributes.GetNamedItem("lon").Value.Replace('.', ',')));
            long id = long.Parse( node.Attributes.GetNamedItem("id").Value );
            List<NodeStructTags> nst = new List<NodeStructTags>();
            if (node.HasChildNodes)
                foreach(XmlNode child in node.ChildNodes)
                    nst.Add(GetStructTagTag(child));
            lns.Add(new NodeStruct() { position = pos, id=id ,structTags = nst.ToArray() });
        }
        nodeStructs = lns.ToArray();
    }

    private string getFloatPoint(float f)
        => f.ToString().Replace(',', '.');

    // Update is called once per frame
    void Update()
    {
        
    }
}
