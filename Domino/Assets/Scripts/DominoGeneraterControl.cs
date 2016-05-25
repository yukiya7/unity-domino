using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DominoGeneraterControl : MonoBehaviour {
    private GameObject game_camera;
    private Vector3[] positions;
    private int position_num = 0;

    public enum STEP
    {

        NONE = -1,

        IDLE = 0,       // 待機中.
        DRAWING,        // ラインを描いている中（ドラッグ中）.
        DRAWED,         // ラインを描き終わった.
        CREATED,        // 道路のモデルが生成された.

        NUM,
    };

    public STEP step = STEP.NONE;
    public STEP next_step = STEP.NONE;

    //線分の最大値
    private static int POSITION_NUM_MAX = 100;

    private DominoBuilder domino_builder;


    private GameObject push_domino;

    private Button push_button;
    void Start()
    {
        game_camera = GameObject.FindGameObjectWithTag("MainCamera");

        //LineRenderer　の線分の数を 0 にしている。
        this.GetComponent<LineRenderer>().SetVertexCount(0);

        this.positions = new Vector3[POSITION_NUM_MAX];

        domino_builder = GameObject.Find("DominoBuilder").GetComponent<DominoBuilder>();

        push_button = GameObject.Find("PushButton").GetComponent<Button>();
    }

    // Update is called once per frame
    void Update()
    {

        // 状態遷移チェック.

        switch (this.step)
        {

            case STEP.NONE:
                {
                    this.next_step = STEP.IDLE;
                }
                break;

            case STEP.IDLE:
                {
                    if (Input.GetMouseButton(0))
                    {

                        this.next_step = STEP.DRAWING;
                    }
                }
                break;

            case STEP.DRAWING:
                {
                    if (!Input.GetMouseButton(0))
                    {

                        if (this.position_num >= 2)
                        {

                            this.next_step = STEP.DRAWED;

                        }
                        else
                        {

                            this.next_step = STEP.IDLE;
                        }
                    }
                }
                break;

            case STEP.DRAWED:
                {
                    push_button.interactable = true;
                }
                break;
        }


        // 状態が遷移したときの初期化.

        if (this.next_step != STEP.NONE)
        {

            switch (this.next_step)
            {

                case STEP.IDLE:
                    {
                        // 前回作成したものを削除しておく.


                        this.position_num = 0;

                        this.GetComponent<LineRenderer>().SetVertexCount(0);

                        domino_builder.RemoveDominos();
                    }
                    break;

                case STEP.CREATED:
                    {
                        //CreateButton を押すとここに来る
                        Debug.Log("CREATED.");
                        domino_builder.positions = this.positions;
                        domino_builder.position_num = this.position_num;
                        push_domino = domino_builder.createDomino();
                    }
                    break;
            }

            this.step = this.next_step;

            this.next_step = STEP.NONE;
        }






        // 各状態での処理.

        switch (this.step)
        {

            case STEP.DRAWING:
                {
                    Vector3 position = this.unproject_mouse_position();

                    // 頂点をラインに追加するか、チェックする.

                    bool is_append_position = false;

                    if (this.position_num == 0)
                    {

                        // 最初のいっこは無条件に追加.

                        is_append_position = true;

                    }
                    else if (this.position_num >= POSITION_NUM_MAX)
                    {

                        // 最大個数をオーバーした時は追加できない.

                        is_append_position = false;

                    }
                    else
                    {

                        // 直前に追加した頂点から一定距離離れたら追加.

                        if (Vector3.Distance(this.positions[this.position_num - 1], position) > 0.5f)
                        {

                            is_append_position = true;
                        }
                    }

                    //

                    if (is_append_position)
                    {

                        if (this.position_num > 0)
                        {

                            Vector3 distance = position - this.positions[this.position_num - 1];

                            distance *= 0.5f / distance.magnitude;

                            position = this.positions[this.position_num - 1] + distance;
                        }

                        this.positions[this.position_num] = position;

                        this.position_num++;

                        // LineRender を作り直しておく.

                        this.GetComponent<LineRenderer>().SetVertexCount(this.position_num);

                        for (int i = 0; i < this.position_num; i++)
                        {

                            this.GetComponent<LineRenderer>().SetPosition(i, this.positions[i]);
                        }
                    }
                }
                break;
        }

    }

    // 『create』ボタンを押したとき.
    public void onCreateButtonPressed()
    {
        if (this.step == STEP.DRAWED)
        {

            this.next_step = STEP.CREATED;
        }
    }

    // 『clear』ボタンを押したとき.
    public void onRemoveButtonPressed()
    {
        if (this.step == STEP.DRAWED)
        {

            this.next_step = STEP.IDLE;
        }

        if (this.step == STEP.CREATED)
        {
            this.next_step = STEP.IDLE;
        }
    }

    // 『push』ボタンを押したとき.
    public void onPushButtonPressed()
    {
        push_domino.GetComponent<Rigidbody>().AddForce(push_domino.transform.forward, ForceMode.VelocityChange);
    }

    // マウスの位置を、３D空間のワールド座標に変換する.
    //
    // ・マウスカーソルとカメラの位置を通る直線
    // ・ピースの中心を通る、水平な面
    //　↑の二つが交わるところを求めます.
    //
    private Vector3 unproject_mouse_position()
    {
        Vector3 mouse_position = Input.mousePosition;

        // ピースの中心を通る、水平（法線がY軸。XZ平面）な面.
        Plane plane = new Plane(Vector3.up, new Vector3(0.0f, 0.0f, 0.0f));

        // カメラ位置とマウスカーソルの位置を通る直線.
        Ray ray = this.game_camera.GetComponent<Camera>().ScreenPointToRay(mouse_position);

        // 上の二つが交わるところを求める.

        float depth;

        plane.Raycast(ray, out depth);

        Vector3 world_position;

        world_position = ray.origin + ray.direction * depth;

        return (world_position);
    }

}
