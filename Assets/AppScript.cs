using System.Collections;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AppScript : MonoBehaviour
{
    [SerializeField] string URL;
    Text text;
    bool fail;//GPS허용 상태를 의미하는 변수
    public void OnClick()//버튼이 클릭했을떄 실행되는 메소드
    {
        if(fail)
            StartCoroutine(StartLocationService());
        if(!fail)
            StartCoroutine(GetLocal());
    }
    void Start(){
        text=GetComponent<Text>();
        StartCoroutine(StartLocationService());
    }
     IEnumerator StartLocationService()
    {
        this.fail=true;
        if (Application.platform == RuntimePlatform.Android)//안드로이드 인가?
        {
            if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))//허용을 못받았는가?
            {
                Permission.RequestUserPermission(Permission.FineLocation);//허용을 받을지 물어본다
            }
        }else{//안드로이드가 아니면?
            text.text="안드로이드 폰으로 실행해주세요.";
            yield break;
        }
        if (!Input.location.isEnabledByUser)//GPS를 활성화 하지않고 허용하지 않으면?
        {
            text.text = "GPS를 활성화 해주세요.";
            yield break;
        }

        Input.location.Start();//GPS 서비스 시작

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait <= 0)
        {
            text.text = "GPS 초기화에 실패했습니다.";
            yield break;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            text.text = "위치를 가져올 수 없습니다.";
            yield break;
        }
        else
        {
            fail=false;
            // GPS 초기화가 성공했을 때
            text.text = "GPS 활성화 성공. 버튼을 눌러주세요";
        }
    }
    IEnumerator GetLocal()
    {
        if (!Input.location.isEnabledByUser)//GPS가 활성화 돼지 않으면?
        {
            text.text = "GPS를 활성화 해주세요.";
            yield break;
        }
            WWWForm form = new WWWForm();//fastapi에 올릴 폼을 만든다
            form.AddField("x", Input.location.lastData.longitude.ToString());//x에 경도값을 넣는다
            form.AddField("y", Input.location.lastData.latitude.ToString());//y에 위도값을 넣는다
            using (UnityWebRequest www = UnityWebRequest.Post(URL + "/getlocal", form))//post를 써서 form데이터를 보낸다
            {
                yield return www.SendWebRequest();//요청이 될떄까지 기다린다
                if (www.result == UnityWebRequest.Result.Success)//접속이 성공하면
                {
                    Json result = JsonUtility.FromJson<Json>(www.downloadHandler.text);//json에 저장한다
                    text.text = result.local;//json에 저장한 loalc값을 출력한다
                }
                else
                    text.text = "오류입니다. 다시 해주시길 바랍니다.";//다시하라고 한다
            }
        }
    
}
