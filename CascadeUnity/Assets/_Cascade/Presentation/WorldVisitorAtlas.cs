using UnityEngine;
namespace Cascade.Presentation
{
    // Measured source-pixel bounds; shared by scene visitors and catalogue previews.
    internal static class WorldVisitorAtlas
    {
        public const float Width=1024,Height=1536;
        internal readonly struct Cell
        {
            public readonly Rect Pixels;public readonly Vector2 BodyPivot;public readonly float BodyLength;
            public Cell(float x,float y,float width,float height,float bodyX,float bodyY,float length){Pixels=new Rect(x,y,width,height);BodyPivot=new Vector2(bodyX,bodyY);BodyLength=length;}
        }
        public static readonly Cell[] Cells={
            new Cell(42,12,177,145,160,114,86),
            new Cell(298,58,177,98,419,114,86),
            new Cell(548,85,178,104,675,114,86),
            new Cell(809,23,178,134,931,114,86),
            new Cell(33,177,189,131,160,279,111),
            new Cell(295,185,185,123,416,279,111),
            new Cell(544,227,191,83,674,279,111),
            new Cell(804,185,188,123,930,279,111),
            new Cell(44,337,163,119,162,421,65),
            new Cell(304,347,163,108,415,421,65),
            new Cell(559,364,164,100,674,421,65),
            new Cell(819,336,160,120,932,421,65),
            new Cell(30,463,209,179,166,603,91),
            new Cell(295,497,200,144,426,603,91),
            new Cell(549,521,206,123,682,603,91),
            new Cell(810,456,197,187,945,603,91),
            new Cell(29,651,213,155,166,770,94),
            new Cell(292,678,211,135,427,770,94),
            new Cell(544,726,213,117,684,770,94),
            new Cell(805,652,205,155,943,770,94),
            new Cell(50,830,162,156,167,941,65),
            new Cell(308,839,163,145,424,941,65),
            new Cell(563,869,167,117,681,941,65),
            new Cell(837,831,146,152,940,941,65),
            new Cell(31,993,186,167,164,1111,88),
            new Cell(292,999,187,163,423,1111,88),
            new Cell(552,1055,191,107,680,1111,88),
            new Cell(820,992,179,167,937,1111,88),
            new Cell(49,1163,155,139,158,1268,66),
            new Cell(309,1191,152,116,416,1268,66),
            new Cell(555,1232,158,87,673,1268,66),
            new Cell(827,1167,147,137,931,1268,66),
            new Cell(20,1313,218,193,168,1430,69),
            new Cell(277,1329,213,178,427,1430,69),
            new Cell(532,1374,214,137,684,1430,69),
            new Cell(791,1319,213,187,942,1430,69)
        };
    }
}
