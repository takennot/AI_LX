using UnityEngine;
using Random = UnityEngine.Random;

namespace L3
{
    public class AgentSpawner : MonoBehaviour
    {
        public SteeringAgent agentPrefab;
        public int agentCount = 10;
        public Vector2 spawnAreaSize = new Vector2(10f, 10f);
    
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            for (int i = 0; i < agentCount; i++)
            {
                Vector3 offset = new Vector3(Random.Range(-spawnAreaSize.x * 0.5f, spawnAreaSize.x * 0.5f), 0f, Random.Range(-spawnAreaSize.y * 0.5f, spawnAreaSize.y * 0.5f));
                Vector3 spawnPosition = offset + transform.position;
                Instantiate(agentPrefab, spawnPosition, Quaternion.identity);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, new Vector3(spawnAreaSize.x, 0.1f, spawnAreaSize.y));
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
