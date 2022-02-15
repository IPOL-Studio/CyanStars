using UnityEngine;

public class InputKeyControl : MonoBehaviour
{
    public GameObject PerfabKey;

    //键盘操作部分长度，单位毫米，一般是按键“Q”左缘到“]”左缘的长度。
    public float keyBoardLength = 209;

    //下面用来输入各个按键左缘的位置……肝疼
    public float posQ = 0;
    public float posW = 19;
    public float posE = 38;
    public float posR = 57;
    public float posT = 76;
    public float posY = 95;
    public float posU = 114;
    public float posI = 133;
    public float posO = 152;
    public float posP = 171;
    public float posLPare = 190;
    public float posRPare = 209;

    public float posA = 5;
    public float posS = 24;
    public float posD = 43;
    public float posF = 62;
    public float posG = 81;
    public float posH = 100;
    public float posJ = 119;
    public float posK = 138;
    public float posL = 157;
    public float posSemi = 176;
    public float posQuot = 195;

    public float posZ = 15;
    public float posX = 34;
    public float posC = 53;
    public float posV = 72;
    public float posB = 91;
    public float posN = 110;
    public float posM = 129;
    public float posComm = 148;
    public float posPeri = 167;
    public float posSlas = 186;

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
    }

    // Update is called once per frame
    void Update()
    {
        //有没有什么优化方案呢（思考）
        #region 第一排
        if (Input.GetKeyDown("q"))
        {
            GameObject key = Instantiate(PerfabKey);
            key.name = "q";
            key.transform.position = new Vector3(28 * posQ / keyBoardLength - 14, 0.02f, -50f);
        }

        if (Input.GetKeyUp("q"))
        {
            GameObject.Destroy(GameObject.Find("q"));
        }

        if (Input.GetKeyDown("w"))
        {
            GameObject key = Instantiate(PerfabKey);
            key.name = "w";
            key.transform.position = new Vector3(28 * posW / keyBoardLength - 14, 0.02f, -50f);
        }

        if (Input.GetKeyUp("w"))
        {
            GameObject.Destroy(GameObject.Find("w"));
        }

        if (Input.GetKeyDown("e"))
        {
            GameObject key = Instantiate(PerfabKey);
            key.name = "e";
            key.transform.position = new Vector3(28 * posE / keyBoardLength - 14, 0.02f, -50f);
        }

        if (Input.GetKeyUp("e"))
        {
            GameObject.Destroy(GameObject.Find("e"));
        }

        if (Input.GetKeyDown("r"))
        {
            GameObject key = Instantiate(PerfabKey);
            key.name = "r";
            key.transform.position = new Vector3(28 * posR / keyBoardLength - 14, 0.02f, -50f);
        }

        if (Input.GetKeyUp("r"))
        {
            GameObject.Destroy(GameObject.Find("r"));
        }

        if (Input.GetKeyDown("t"))
        {
            GameObject key = Instantiate(PerfabKey);
            key.name = "t";
            key.transform.position = new Vector3(28 * posT / keyBoardLength - 14, 0.02f, -50f);
        }

        if (Input.GetKeyUp("t"))
        {
            GameObject.Destroy(GameObject.Find("t"));
        }

        if (Input.GetKeyDown("y"))
        {
            GameObject key = Instantiate(PerfabKey);
            key.name = "y";
            key.transform.position = new Vector3(28 * posY / keyBoardLength - 14, 0.02f, -50f);
        }

        if (Input.GetKeyUp("y"))
        {
            GameObject.Destroy(GameObject.Find("y"));
        }

        if (Input.GetKeyDown("u"))
        {
            GameObject key = Instantiate(PerfabKey);
            key.name = "u";
            key.transform.position = new Vector3(28 * posU / keyBoardLength - 14, 0.02f, -50f);
        }

        if (Input.GetKeyUp("u"))
        {
            GameObject.Destroy(GameObject.Find("u"));
        }

        if (Input.GetKeyDown("i"))
        {
            GameObject key = Instantiate(PerfabKey);
            key.name = "i";
            key.transform.position = new Vector3(28 * posI / keyBoardLength - 14, 0.02f, -50f);
        }

        if (Input.GetKeyUp("i"))
        {
            GameObject.Destroy(GameObject.Find("i"));
        }

        if (Input.GetKeyDown("o"))
        {
            GameObject key = Instantiate(PerfabKey);
            key.name = "o";
            key.transform.position = new Vector3(28 * posO / keyBoardLength - 14, 0.02f, -50f);
        }

        if (Input.GetKeyUp("o"))
        {
            GameObject.Destroy(GameObject.Find("o"));
        }

        if (Input.GetKeyDown("p"))
        {
            GameObject key = Instantiate(PerfabKey);
            key.name = "p";
            key.transform.position = new Vector3(28 * posP / keyBoardLength - 14, 0.02f, -50f);
        }

        if (Input.GetKeyUp("p"))
        {
            GameObject.Destroy(GameObject.Find("p"));
        }

        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            GameObject key = Instantiate(PerfabKey);
            key.name = "LeftBracket";
            key.transform.position = new Vector3(28 * posLPare / keyBoardLength - 14, 0.02f, -50f);
        }

        if (Input.GetKeyUp(KeyCode.LeftBracket))
        {
            GameObject.Destroy(GameObject.Find("LeftBracket"));
        }

        if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            GameObject key = Instantiate(PerfabKey);
            key.name = "RightBracket";
            key.transform.position = new Vector3(28 * posRPare / keyBoardLength - 14, 0.02f, -50f);
        }

        if (Input.GetKeyUp(KeyCode.RightBracket))
        {
            GameObject.Destroy(GameObject.Find("RightBracket"));
        }
        #endregion
        #region 第二排
        if (Input.GetKeyDown("a"))
        {
            GameObject key = Instantiate(PerfabKey);
            key.name = "a";
            key.transform.position = new Vector3(28 * posA / keyBoardLength - 14, 0.02f, -50f);
        }

        if (Input.GetKeyUp("a"))
        {
            GameObject.Destroy(GameObject.Find("a"));
        }

        if (Input.GetKeyDown("s"))
        {
            GameObject key = Instantiate(PerfabKey);
            key.name = "s";
            key.transform.position = new Vector3(28 * posS / keyBoardLength - 14, 0.02f, -50f);
        }

        if (Input.GetKeyUp("s"))
        {
            GameObject.Destroy(GameObject.Find("s"));
        }

        if (Input.GetKeyDown("d"))
        {
            GameObject key = Instantiate(PerfabKey);
            key.name = "d";
            key.transform.position = new Vector3(28 * posD / keyBoardLength - 14, 0.02f, -50f);
        }

        if (Input.GetKeyUp("d"))
        {
            GameObject.Destroy(GameObject.Find("d"));
        }

        if (Input.GetKeyDown("f"))
        {
            GameObject key = Instantiate(PerfabKey);
            key.name = "f";
            key.transform.position = new Vector3(28 * posF / keyBoardLength - 14, 0.02f, -50f);
        }

        if (Input.GetKeyUp("f"))
        {
            GameObject.Destroy(GameObject.Find("f"));
        }

        if (Input.GetKeyDown("g"))
        {
            GameObject key = Instantiate(PerfabKey);
            key.name = "g";
            key.transform.position = new Vector3(28 * posG / keyBoardLength - 14, 0.02f, -50f);
        }

        if (Input.GetKeyUp("g"))
        {
            GameObject.Destroy(GameObject.Find("g"));
        }

        if (Input.GetKeyDown("h"))
        {
            GameObject key = Instantiate(PerfabKey);
            key.name = "h";
            key.transform.position = new Vector3(28 * posH / keyBoardLength - 14, 0.02f, -50f);
        }

        if (Input.GetKeyUp("h"))
        {
            GameObject.Destroy(GameObject.Find("h"));
        }

        if (Input.GetKeyDown("j"))
        {
            GameObject key = Instantiate(PerfabKey);
            key.name = "j";
            key.transform.position = new Vector3(28 * posJ / keyBoardLength - 14, 0.02f, -50f);
        }

        if (Input.GetKeyUp("j"))
        {
            GameObject.Destroy(GameObject.Find("j"));
        }

        if (Input.GetKeyDown("k"))
        {
            GameObject key = Instantiate(PerfabKey);
            key.name = "k";
            key.transform.position = new Vector3(28 * posK / keyBoardLength - 14, 0.02f, -50f);
        }

        if (Input.GetKeyUp("k"))
        {
            GameObject.Destroy(GameObject.Find("k"));
        }

        if (Input.GetKeyDown("l"))
        {
            GameObject key = Instantiate(PerfabKey);
            key.name = "l";
            key.transform.position = new Vector3(28 * posL / keyBoardLength - 14, 0.02f, -50f);
        }

        if (Input.GetKeyUp("l"))
        {
            GameObject.Destroy(GameObject.Find("l"));
        }

        if (Input.GetKeyDown(KeyCode.Semicolon) || Input.GetKeyDown(KeyCode.Colon))
        {
            GameObject key = Instantiate(PerfabKey);
            key.name = "SE";
            key.transform.position = new Vector3(28 * posSemi / keyBoardLength - 14, 0.02f, -50f);
        }

        if (Input.GetKeyUp(KeyCode.Semicolon) || Input.GetKeyUp(KeyCode.Colon))
        {
            GameObject.Destroy(GameObject.Find("SE"));
        }

        if (Input.GetKeyDown(KeyCode.Quote) || Input.GetKeyDown(KeyCode.DoubleQuote))
        {
            GameObject key = Instantiate(PerfabKey);
            key.name = "QU";
            key.transform.position = new Vector3(28 * posQuot / keyBoardLength - 14, 0.02f, -50f);
        }

        if (Input.GetKeyUp(KeyCode.Quote) || Input.GetKeyUp(KeyCode.DoubleQuote))
        {
            GameObject.Destroy(GameObject.Find("QU"));
        }
        #endregion
        #region 第三排
        if (Input.GetKeyDown("z"))
        {
            GameObject key = Instantiate(PerfabKey);
            key.name = "z";
            key.transform.position = new Vector3(28 * posZ / keyBoardLength - 14, 0.02f, -50f);
        }

        if (Input.GetKeyUp("z"))
        {
            GameObject.Destroy(GameObject.Find("z"));
        }

        if (Input.GetKeyDown("x"))
        {
            GameObject key = Instantiate(PerfabKey);
            key.name = "x";
            key.transform.position = new Vector3(28 * posX / keyBoardLength - 14, 0.02f, -50f);
        }

        if (Input.GetKeyUp("x"))
        {
            GameObject.Destroy(GameObject.Find("x"));
        }

        if (Input.GetKeyDown("c"))
        {
            GameObject key = Instantiate(PerfabKey);
            key.name = "c";
            key.transform.position = new Vector3(28 * posC / keyBoardLength - 14, 0.02f, -50f);
        }

        if (Input.GetKeyUp("c"))
        {
            GameObject.Destroy(GameObject.Find("c"));
        }

        if (Input.GetKeyDown("v"))
        {
            GameObject key = Instantiate(PerfabKey);
            key.name = "v";
            key.transform.position = new Vector3(28 * posV / keyBoardLength - 14, 0.02f, -50f);
        }

        if (Input.GetKeyUp("v"))
        {
            GameObject.Destroy(GameObject.Find("v"));
        }

        if (Input.GetKeyDown("b"))
        {
            GameObject key = Instantiate(PerfabKey);
            key.name = "b";
            key.transform.position = new Vector3(28 * posB / keyBoardLength - 14, 0.02f, -50f);
        }

        if (Input.GetKeyUp("b"))
        {
            GameObject.Destroy(GameObject.Find("b"));
        }

        if (Input.GetKeyDown("n"))
        {
            GameObject key = Instantiate(PerfabKey);
            key.name = "n";
            key.transform.position = new Vector3(28 * posN / keyBoardLength - 14, 0.02f, -50f);
        }

        if (Input.GetKeyUp("n"))
        {
            GameObject.Destroy(GameObject.Find("n"));
        }

        if (Input.GetKeyDown("m"))
        {
            GameObject key = Instantiate(PerfabKey);
            key.name = "m";
            key.transform.position = new Vector3(28 * posM / keyBoardLength - 14, 0.02f, -50f);
        }

        if (Input.GetKeyUp("m"))
        {
            GameObject.Destroy(GameObject.Find("m"));
        }

        if (Input.GetKeyDown(KeyCode.Comma) || Input.GetKeyDown(KeyCode.Less))
        {
            GameObject key = Instantiate(PerfabKey);
            key.name = "CO";
            key.transform.position = new Vector3(28 * posComm / keyBoardLength - 14, 0.02f, -50f);
        }

        if (Input.GetKeyUp(KeyCode.Comma) || Input.GetKeyDown(KeyCode.Less))
        {
            GameObject.Destroy(GameObject.Find("CO"));
        }

        if (Input.GetKeyDown(KeyCode.Period) || Input.GetKeyDown(KeyCode.Greater))
        {
            GameObject key = Instantiate(PerfabKey);
            key.name = "PE";
            key.transform.position = new Vector3(28 * posPeri / keyBoardLength - 14, 0.02f, -50f);
        }

        if (Input.GetKeyUp(KeyCode.Period) || Input.GetKeyUp(KeyCode.Greater))
        {
            GameObject.Destroy(GameObject.Find("PE"));
        }

        if (Input.GetKeyDown(KeyCode.Slash) || Input.GetKeyDown(KeyCode.Question))
        {
            GameObject key = Instantiate(PerfabKey);
            key.name = "SL";
            key.transform.position = new Vector3(28 * posSlas / keyBoardLength - 14, 0.02f, -50f);
        }

        if (Input.GetKeyUp(KeyCode.Slash) || Input.GetKeyUp(KeyCode.Question))
        {
            GameObject.Destroy(GameObject.Find("SL"));
        }
        #endregion


    }

    
}

