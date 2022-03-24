
using UnityEngine;
using System.Collections;

namespace TBT.GemsAndCombos {

    public class GemHandler : MonoBehaviour {

        public bool mouseGem = false;
        public Vector2 dropTarget = Vector2.zero;

        private void Update () 
        {
            GemMove();
        }

        void GemMove()
        {
            if (!mouseGem) return;

            //드랍이 마우스 따라가게
            transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition) + new Vector3(0, 0, 5f);

            //마우스 클릭 떄면 
            if (Input.GetMouseButtonUp(0))
            {
                mouseGem = false;
            }
        }


        //처음 생성될떄 드롭이 5만큼 위로 올라가 생성되있음
        //생성후 initDrop을 통해 아래로 내림
        public void InitDrop (int dropDistance) {
            dropTarget = new Vector2(transform.position.x, transform.position.y - dropDistance);

            StartCoroutine(DropGem());
        }

        private IEnumerator DropGem () {
            WaitForSeconds frameTime = new WaitForSeconds(0.01f);
            Vector2 startPos = transform.position;
            float lerpPercent = 0;

            while (lerpPercent <= 1) {
                transform.position = Vector2.Lerp(startPos, dropTarget, lerpPercent);
                lerpPercent += 0.05f;
                yield return frameTime;
            }

            transform.position = dropTarget;
        }
    }
}