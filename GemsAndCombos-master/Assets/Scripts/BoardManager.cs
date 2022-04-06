using UnityEngine;
using System.Collections;
using System;

namespace TBT.GemsAndCombos {
    public class BoardManager : MonoBehaviour 
    {
        //드랍 위치값 오브젝트 스크립트 저장
        private struct Gem {
            public int color;
            public GameObject gemObject;
            public GemHandler GM;
            public int dropDistance;
            public int matchNumber;
        }

        private Gem[,] board = new Gem[6, 5];
        private Gem gemClone = new Gem();

        [SerializeField] private Material mFire;
        [SerializeField] private Material mWater;
        [SerializeField] private Material mWood;
        [SerializeField] private Material mLight;
        [SerializeField] private Material mDark;
        [SerializeField] private Material mHeart;

        private Vector2 mousePos = Vector2.zero;
        private int heldGemX = 0, heldGemY = 0;
        private bool boardLocked = false;
        private bool mouseDown = false;


        private void Start () 
        {
            //드랍 생성
            InitBoard();
        }

        private void Update () 
        {
            if (boardLocked) return;

            if (Input.GetMouseButtonDown(0)) {
                mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mouseDown = true;
                CheckGemClick();
            } else if (Input.GetMouseButton(0) && mouseDown) {
                mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                CheckGemSwap();
            }

            if (Input.GetMouseButtonUp(0) && mouseDown) {
                mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                DropGem();
                boardLocked = true;
                StartCoroutine(MatchGems());
                mouseDown = false;
            }
        }



        private void InitBoard () 
        {
            //반복문으로 x,y(드롭 위치 지정)
            for (int y = 0; y < 5; y++) 
            {
                for (int x = 0; x < 6; x++) 
                {
                    SpawnGem(x, y, 5);
                }
            }

            //드롭이 반복문으로 모두 생성되고나면
            //드롭을 아래로 내림
            SkyfallGems();
        }

        //드랍생성
        private void SpawnGem (int x, int y, int drop) 
        {
            //위에서 떨어지는 연출을 위해 y값은 5를 더해서 생성
            GameObject gem = Instantiate(Resources.Load("Gem") as GameObject, new Vector2(x, y + 5), Quaternion.identity);

            //중복생성 방지
            if (board[x, y].gemObject != null)
            {
                Destroy(board[x, y].gemObject);
            }

            board[x, y] = new Gem 
            {
                gemObject = gem,
                dropDistance = drop,
                GM = gem.GetComponent<GemHandler>(),
                matchNumber = 0
            };

            SetColor(x, y);
        }

        //드랍 색 설정
        private void SetColor (int x, int y)
        {
            int randomColor = UnityEngine.Random.Range(0, 6);

            switch (randomColor) 
            {
                case 0: // fire
                    board[x, y].gemObject.transform.GetChild(0).GetComponent<MeshRenderer>().material = mFire;
                    break;
                case 1: // water
                    board[x, y].gemObject.transform.GetChild(0).GetComponent<MeshRenderer>().material = mWater;
                    break;
                case 2: // wood
                    board[x, y].gemObject.transform.GetChild(0).GetComponent<MeshRenderer>().material = mWood;
                    break;
                case 3: // light
                    board[x, y].gemObject.transform.GetChild(0).GetComponent<MeshRenderer>().material = mLight;
                    break;
                case 4: // dark
                    board[x, y].gemObject.transform.GetChild(0).GetComponent<MeshRenderer>().material = mDark;
                    break;
                case 5: // heart
                    board[x, y].gemObject.transform.GetChild(0).GetComponent<MeshRenderer>().material = mHeart;
                    break;
            }

            board[x, y].color = randomColor;
        }

        //드랍 첫 생성 후 드랍을 아래로 움직이게 만듬
        private void SkyfallGems () 
        {
            for (int y = 0; y < 5; y++)
            {
                for (int x = 0; x < 6; x++) 
                {
                    if (board[x, y].dropDistance > 0)
                    {
                        //GenHandler의 initDrop을 작동
                        board[x, y].GM.InitDrop(board[x, y].dropDistance);
                        board[x, y].dropDistance = 0;
                    }
                }
            }
        }

        //CheckGemClick, CheckGemSwap에서 사용
        delegate void setXY(int a, int b);
        Action mouseGemBoolChagner;

        //마우스 클릭으로 드랍 들기
        private void CheckGemClick () 
        {
            ////마우스 클릭시의 마우스 위치값을
            //RoundToint을 사용해 반올림하여 대입
            int clickX = Mathf.RoundToInt(mousePos.x);
            int clickY = Mathf.RoundToInt(mousePos.y);

            //델리게이트 setXY에 무명함수를 람다식으로 대입
            //위에서 반올림해 대입한 값이 보드의 최대 최소값보다 크거나 작을경우 최대 최소값으로 고정
            setXY clickXY = (x, y) =>
            {
                if (x > 5) x = 5;
                if (y > 4) y = 4;
                if (x < 0) x = 0;
                if (y < 0) y = 0;
            };

            clickXY(clickX, clickY);


            //클릭한 위치에 있는 드랍의 bool값 mouseGem을 true로 바꿔 마우스를 따라다니게 만듬
            mouseGemBoolChagner = () => board[clickX, clickY].GM.mouseGem = true;
            mouseGemBoolChagner();

            //아래의 CheckGemSwap에서 드랍 스왑시 사용할
            //마우스를 클릭해 드랍을 들어올린 위치값을 대입
            heldGemX = clickX;
            heldGemY = clickY;

            //들고있는 드랍이 보드에 위치하게될곳을 표시하기위해 클론을 생성함
            CreateNewGemClone(clickX, clickY);

            //클론 드랍의 투명도 조절
            Material gemMat = board[clickX, clickY].gemObject.transform.GetChild(0).GetComponent<MeshRenderer>().material;
            Color32 gemCol = gemMat.color;
            gemCol.a = 60;

            gemClone.gemObject.transform.GetChild(0).GetComponent<MeshRenderer>().material = gemMat;
            gemClone.gemObject.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = gemCol;
        }

        delegate void mouseXY(int a, int b);
        //들고있는 드랍 이동시 드랍끼리의 위치 변경
        private void CheckGemSwap () 
        {
            //Roundtoint로 반올림 후 대입
            int mouseX = Mathf.RoundToInt(mousePos.x);
            int mouseY = Mathf.RoundToInt(mousePos.y);

            //반올림한 값이 보드의 최대 최소값보다 크거나 작을 경우 최대 최소값으로 고정
            /*mouseXY clickXY = (x, y) =>
            {
                if (x > 5) x = 5;
                if (y > 4) y = 4;
                if (x < 0) x = 0;
                if (y < 0) y = 0;
            };

            clickXY(mouseX, mouseY);*/

            if (mouseX > 5) mouseX = 5;
            if (mouseY > 4) mouseY = 4;
            if (mouseX < 0) mouseX = 0;
            if (mouseY < 0) mouseY = 0;

            //위 CheckGemClick에서 대입받은 드랍을 클릭했던 위치와 현재 마우스의 위치가 다를 경우
            //코루틴을 통해 클론드랍의 위치를 해당 위치로 이동
            if (mouseX != heldGemX || mouseY != heldGemY) 
            {
                if (mouseX - heldGemX > 1) mouseX = heldGemX + 1;
                if (heldGemX - mouseX > 1) mouseX = heldGemX - 1;
                if (mouseY - heldGemY > 1) mouseY = heldGemY + 1;
                if (heldGemY - mouseY > 1) mouseY = heldGemY - 1;

                
                StartCoroutine(SwapGems(mouseX, mouseY));
            }
        }

        private IEnumerator SwapGems (int newGemX, int newGemY) 
        {
            Vector3 targetAngle;
            GameObject gemSwapper = new GameObject();
            WaitForSeconds swapLoopTimer = new WaitForSeconds(0.01f);
            float swapLerpPercent = 0f;
            //드랍을 클릭했던 위치값(현제 플레이어가 들고있는 드랍의 위치값)을 oldGemXY에 대입
            int oldGemX = heldGemX, oldGemY = heldGemY;
            //heldGemXY에는 newGemXY(위에서 heldGemXY에서 1을 더하고 뺀) 이동한 위치에 있는 드랍의 위치값
            heldGemX = newGemX; heldGemY = newGemY;

            Gem tempGem = board[newGemX, newGemY];
            board[newGemX, newGemY] = board[oldGemX, oldGemY];
            board[oldGemX, oldGemY] = tempGem;

            //클론 드랍의 위치를 현재 마우스의 위치로
            if (gemClone.gemObject.transform.parent != null) {
                gemClone.gemObject.transform.parent = null;
                gemClone.gemObject.transform.position = new Vector2(oldGemX, oldGemY);
            }

            //
           /* if (board[oldGemX, oldGemY].gemObject.transform.parent != null) {
                board[oldGemX, oldGemY].gemObject.transform.parent = null;
                board[oldGemX, oldGemY].gemObject.transform.position = new Vector2(newGemX, newGemY);
            }*/

            targetAngle = new Vector3(0, 0, 180f);
            gemSwapper.transform.position = new Vector2(oldGemX - ((oldGemX - newGemX) / 2f), oldGemY - ((oldGemY - newGemY) / 2f));

            gemClone.gemObject.transform.parent = board[oldGemX, oldGemY].gemObject.transform.parent = gemSwapper.transform;

            while (swapLerpPercent <= 1f) {
                gemSwapper.transform.eulerAngles = Vector3.Lerp(Vector3.zero, targetAngle, swapLerpPercent);
                swapLerpPercent += 0.1f;
                yield return swapLoopTimer;
            }

            gemSwapper.transform.eulerAngles = targetAngle;

            if (board[oldGemX, oldGemY].gemObject.transform.parent == gemSwapper.transform)
                board[oldGemX, oldGemY].gemObject.transform.parent = null;
            if (gemClone.gemObject != null)
                if (gemClone.gemObject.transform.parent == gemSwapper.transform)
                    gemClone.gemObject.transform.parent = null;

            Destroy(gemSwapper);
        }

        //마우스 클릭을 중지하면 작동
        private void DropGem () 
        {
            //들고있는 드랍의 mouseGen을 false로 변경해 마우스를 더 이상 따라다니지 않게 만듬
            mouseGemBoolChagner = () => board[heldGemX, heldGemY].GM.mouseGem = false;
            mouseGemBoolChagner();

            board[heldGemX, heldGemY].gemObject.transform.position = new Vector2(heldGemX, heldGemY);
            if (gemClone.gemObject != null)
                Destroy(gemClone.gemObject);
        }

        //드랍 클론 생성
        private void CreateNewGemClone (int cloneX, int cloneY) 
        {
            //받아온 X,Y값을 이용해 위치를 지정
            GameObject gem = Instantiate(Resources.Load("Gem") as GameObject, new Vector2(cloneX, cloneY), Quaternion.identity);
            gemClone = new Gem 
            {
                color = board[cloneX, cloneY].color,
                gemObject = gem,
                GM = gem.GetComponent<GemHandler>()
            };
        }

        private IEnumerator MatchGems () {
            bool matchMade = false;
            int matchCount = 0, oldMatchCount = 0;

            // Slight beginning delay
            yield return new WaitForSeconds(0.5f);
            
            for (int y = 0; y < 5; y++) {
                for (int x = 0; x < 6; x++) {
                    int currentColor = board[x, y].color;
                    int oldMatchFound = 0;

                    // Checking for matches to the right
                    if (x < 4) {
                        int z = 1;
                        
                        while (x + z < 6) {
                            if (board[x + z, y].color == currentColor) {
                                if (board[x + z, y].matchNumber > 0 && (oldMatchFound > board[x + z, y].matchNumber || oldMatchFound == 0))
                                    oldMatchFound = board[x + z, y].matchNumber;
                                z++;
                            } else break;
                        }

                        if (z > 2) {
                            matchMade = true;
                            if (matchCount == oldMatchCount) matchCount++;
                            for (int i = 0; i < z; i++) {
                                if (oldMatchFound > 0)
                                    board[x + i, y].matchNumber = oldMatchFound;
                                else board[x + i, y].matchNumber = matchCount;
                            }
                        }
                    }

                    // Checking for matches to the left
                    if (x > 1) {
                        int z = 1;

                        while (x - z > -1) {
                            if (board[x - z, y].color == currentColor) {
                                if (board[x - z, y].matchNumber > 0 && (oldMatchFound > board[x - z, y].matchNumber || oldMatchFound == 0))
                                    oldMatchFound = board[x - z, y].matchNumber;
                                z++;
                            } else break;
                        }

                        if (z > 2) {
                            matchMade = true;
                            if (matchCount == oldMatchCount) matchCount++;
                            for (int i = 0; i < z; i++) {
                                if (oldMatchFound > 0)
                                    board[x - i, y].matchNumber = oldMatchFound;
                                else board[x - i, y].matchNumber = matchCount;
                            }
                        }
                    }

                    // Checking for matches up above
                    if (y < 3) {
                        int z = 1;

                        while (y + z < 5) {
                            if (board[x, y + z].color == currentColor) {
                                if (board[x, y + z].matchNumber > 0 && (oldMatchFound > board[x, y + z].matchNumber || oldMatchFound == 0))
                                    oldMatchFound = board[x, y + z].matchNumber;
                                z++;
                            } else break;
                        }

                        if (z > 2) {
                            matchMade = true;
                            if (matchCount == oldMatchCount) matchCount++;
                            for (int i = 0; i < z; i++) {
                                if (oldMatchFound > 0)
                                    board[x, y + i].matchNumber = oldMatchFound;
                                else board[x, y + i].matchNumber = matchCount;
                            }
                        }
                    }

                    // Checking for matches down below
                    if (y > 1) {
                        int z = 1;

                        while (y - z > -1) {
                            if (board[x, y - z].color == currentColor) {
                                if (board[x, y - z].matchNumber > 0 && (oldMatchFound > board[x, y - z].matchNumber || oldMatchFound == 0))
                                    oldMatchFound = board[x, y - z].matchNumber;
                                z++;
                            } else break;
                        }

                        if (z > 2) {
                            matchMade = true;
                            if (matchCount == oldMatchCount) matchCount++;
                            for (int i = 0; i < z; i++) {
                                if (oldMatchFound > 0)
                                    board[x, y - i].matchNumber = oldMatchFound;
                                else board[x, y - i].matchNumber = matchCount;
                            }
                        }
                    }

                    oldMatchCount = matchCount;
                }
            }

            // Remove all currently matched gems, I'm going to do it a janky way
            if (matchMade) {
                for (int zz = 1; zz <= matchCount; zz++) {
                    for (int yy = 0; yy < 5; yy++) {
                        for (int xx = 0; xx < 6; xx++) {
                            if (board[xx, yy].matchNumber == zz) 
                                DestroyObject(board[xx, yy].gemObject);
                        }
                    }

                    yield return new WaitForSeconds(0.2f);
                }
            
                DropRemainingGems();
                DropNewGems();
            } else {
                yield return new WaitForSeconds(0.25f);
                boardLocked = false;
            }
        }

        private void DropRemainingGems () {
            for (int y = 1; y < 5; y++) {
                for (int x = 0; x < 6; x++) {
                    if (board[x, y].matchNumber > 0) continue;

                    int dropGem = 0;
                    for (int i = 1; i <= y; i++) {
                        if (board[x, y - i].matchNumber > 0)
                            dropGem++;
                    }

                    if (dropGem > 0) {
                        board[x, y].dropDistance = dropGem;

                        Gem tempGem = board[x, y - dropGem];
                        board[x, y - dropGem] = board[x, y];
                        board[x, y] = tempGem;
                    }
                }
            }
        }

        private void DropNewGems () {
            for (int y = 4; y >= 0; y--) {
                for (int x = 0; x < 6; x++) {
                    if (board[x, y].matchNumber > 0) {
                        SpawnGem(x, y, 5);
                    }
                }
            }

            SkyfallGems();
            StartCoroutine(MatchGems());
        }
    }
}