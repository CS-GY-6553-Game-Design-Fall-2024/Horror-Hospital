using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCell : MonoBehaviour
{
    [System.Serializable]
    public struct Wall {
        public GameObject wallCollider;
        public GameObject wallVisual;
    }

    [System.Serializable]
    public class Obstacle {
        public GameObject obstacleMesh;
        public GameObject obstacleCollider;
        public bool randomizePosition;
        public GameObject dependentOn;
    }

    [Header("=== References ===")]
    [SerializeField] private Transform m_meshParent;
    [SerializeField] private Wall[] walls;   // 0 = west, 1 = north, 2 = south, 3 = east
    [SerializeField] private GameObject m_destinationMeshParent;
    [SerializeField] private Material m_dirtyWallMat;
    [SerializeField] private Material m_cleanWallMat;

    [Header("=== Obstacles ===")]
    [SerializeField, Range(0f,1f)] private float m_obstacleSpawnRate = 0.3f;
    [SerializeField] private Obstacle[] m_obstacles;
    [SerializeField] private Obstacle m_activeObstacle;
    [SerializeField, Range(0f,1f)] private float m_lightSpawnRate = 0.5f;
    [SerializeField] private GameObject m_light;
    [SerializeField] private GameObject m_activeLight;
    [SerializeField] private ParticleSystem m_particleSystem;

    [Header("=== Shake Settings ===")]
    [SerializeField] private AnimationCurve m_shakeCurve;
    [SerializeField] private float m_shakeDuration;

    public float debugShakeStrength = 0.05f;

    public void SetWall(int index, bool setTo) {
        walls[index].wallCollider.SetActive(setTo);
        walls[index].wallVisual.SetActive(setTo);
    }

    public void StartShake(float strength=0.1f) {
        StartCoroutine(Shake(strength));
    }
    private IEnumerator Shake(float strength) {
        float startTime = Time.time;
        while(Time.time - startTime <= m_shakeDuration) {
            float diff = Time.time - startTime;
            float durationRatio = diff/m_shakeDuration;
            Vector3 randomPos = Random.insideUnitSphere * m_shakeCurve.Evaluate(durationRatio) * strength;
            m_meshParent.localPosition = randomPos;
            yield return null;
        }
        m_meshParent.localPosition = Vector3.zero;
    }

    public void SetAsDestination() {
        m_destinationMeshParent.SetActive(true);
    }

    public void SetDirtyWalls(bool shouldBeDirty) {
        if (shouldBeDirty) {
            foreach(Wall wall in walls) wall.wallVisual.GetComponent<Renderer>().material = m_dirtyWallMat;
        } else {
            foreach(Wall wall in walls) wall.wallVisual.GetComponent<Renderer>().material = m_cleanWallMat;
        }
    }

    public void CreateObstacle() {
        int obstacleIndex = -1;
        if (UnityEngine.Random.Range(0f,1f) <= m_obstacleSpawnRate) {
            // THe randomization encourages that we produce an obstacle. We then must choose a random obstacle
            obstacleIndex = UnityEngine.Random.Range(0, m_obstacles.Length);
            if (
                m_obstacles[obstacleIndex].dependentOn != null 
                && !m_obstacles[obstacleIndex].dependentOn.activeSelf
            ) {
                obstacleIndex = -1;
            }
        }
        for(int i = 0; i < m_obstacles.Length; i++) {
            bool isActive = (i==obstacleIndex) ? true : false;
            m_obstacles[i].obstacleMesh.SetActive(isActive);
            m_obstacles[i].obstacleCollider.SetActive(isActive);
        }
        if (obstacleIndex != -1) {
            m_activeObstacle = m_obstacles[obstacleIndex];
            if (m_activeObstacle.randomizePosition) {
                Vector3 pos = new Vector3(
                    UnityEngine.Random.Range(-1.5f,1.5f),
                    0f,
                    UnityEngine.Random.Range(-1.5f,1.5f)
                );
                Quaternion rot = Quaternion.Euler(
                    0f, 
                    UnityEngine.Random.Range(0f, 359f), 
                    0f
                );
                m_activeObstacle.obstacleMesh.transform.localPosition = pos;
                m_activeObstacle.obstacleMesh.transform.localRotation = rot;
                m_activeObstacle.obstacleCollider.transform.localPosition = pos;
                m_activeObstacle.obstacleCollider.transform.localRotation = rot;
            }
        }
    }

    public void SetObstacle(bool setAsActive) {
        if (m_activeObstacle != null) {
            if (m_activeObstacle.obstacleMesh != null) m_activeObstacle.obstacleMesh.SetActive(setAsActive);
            if (m_activeObstacle.obstacleCollider != null) m_activeObstacle.obstacleCollider.SetActive(setAsActive);
        }
    }

    public void DeleteObstacle() {
        // Unlike "SetObstacle()", this utterly forces the cell to remove and and all obstacles.
        if (m_activeObstacle != null) {
            if (m_activeObstacle.obstacleMesh != null) m_activeObstacle.obstacleMesh.SetActive(false);
            if (m_activeObstacle.obstacleCollider != null) m_activeObstacle.obstacleCollider.SetActive(false);
            m_activeObstacle = null;
        }
    }

    public void CreateLight() {
        if (UnityEngine.Random.Range(0f,1f) <= m_lightSpawnRate) {
            // THe randomization encourages that we produce a light.
            m_activeLight = m_light;
            m_activeLight.SetActive(true);
        }
    }
    public void DeleteLight() {
        // Unlike "SetObstacle()", this utterly forces the cell to remove the light
        if (m_activeLight != null) {
            m_activeLight.SetActive(false);
            m_activeLight = null;
        }
    }
    public void SetLight(bool setAsActive) {
        if (m_activeLight !=null) m_activeLight.SetActive(setAsActive);
    }

    public void SetParticles(bool activateParticles) {
        m_particleSystem.gameObject.SetActive(activateParticles);
    }
    public bool LightActive(){
        return m_activeLight != null;
    }
}
