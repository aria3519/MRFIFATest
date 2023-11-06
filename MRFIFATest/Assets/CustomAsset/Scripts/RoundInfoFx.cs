using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundInfoFx : MonoBehaviour
{
    [SerializeField] GameObject _roundIcon = null;
    [SerializeField] GameObject _roundFrame = null;
    [SerializeField] List<Sprite> _roundSprites = null;
    [SerializeField] float _LifeTime = 3.0f;
    float _timeCount = 0f;
    int step = 0;

    [SerializeField] bool _reverse = false;

    void Start()
    {
        _timeCount = _LifeTime;
        _roundIcon.SetActive(false);
    }

    void Update()
    {
        roundFrameRotate();
    }

    public void showRoundFx(bool on, int round)
    {
        if (on)
        {
            step = 0;
            _timeCount = 0f;
            _roundIcon.SetActive(true);

            if(round <= 3)
                _roundIcon.GetComponent<SpriteRenderer>().sprite = _roundSprites[round - 1];
            else
                _roundIcon.GetComponent<SpriteRenderer>().sprite = _roundSprites[3];

            if(_reverse)
                _roundIcon.transform.position = GameObject.Find("XR Origin").transform.GetChild(0).transform.GetChild(0).transform.position + Vector3.back * 2f;
            else
                _roundIcon.transform.position = GameObject.Find("XR Origin").transform.GetChild(0).transform.GetChild(0).transform.position + Vector3.forward * 2f;

            _roundIcon.transform.localScale = new Vector3(_roundIcon.transform.localScale.x, 0f, _roundIcon.transform.localScale.z);
        }
        else
        {
            _timeCount = _LifeTime;
            _roundIcon.SetActive(false);
        }
    }

    void roundFrameRotate()
    {
        if (_roundIcon.activeSelf)
        {
            _roundFrame.transform.Rotate(Vector3.back * 150.0f * Time.deltaTime);

            if (step == 0)
            {
                if (_roundIcon.transform.localScale.y < 0.15f)
                    _roundIcon.transform.localScale += Time.deltaTime * Vector3.up * 0.5f;
                else
                {
                    _roundIcon.transform.localScale = Vector3.one * 0.1f + Vector3.up * 0.05f;
                    step = 1;
                }
            }
            else if (step == 1)
            {
                if (_roundIcon.transform.localScale.y > 0.1f)
                    _roundIcon.transform.localScale += Time.deltaTime * Vector3.down * 0.5f;
                else
                {
                    _roundIcon.transform.localScale = Vector3.one * 0.1f;
                    step = 2;
                }
            }

            if (_timeCount < _LifeTime)
                _timeCount += Time.deltaTime;
            else
                _timeCount = _LifeTime;
        }
    }
}
