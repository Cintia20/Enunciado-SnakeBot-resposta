using UnityEngine;
using System.Collections.Generic;

public class SmartBot : AIBehaviour
{
    // Par�metros ajust�veis
    private float wanderRadius = 10f;
    private float wanderDistance = 5f;
    private float wanderJitter = 1f;
    private float detectionRadius = 7f;
    private float obstacleAvoidanceDistance = 3f;

    private List<GameObject> nearbyOrbs = new List<GameObject>();
    private List<GameObject> nearbySnakes = new List<GameObject>();

    public override void Init(GameObject own, SnakeMovement ownMove)
    {
        base.Init(own, ownMove);
        randomPoint = GetRandomWanderPoint();
    }

    public override void Execute()
    {
        // Limpa as listas de objetos detectados
        nearbyOrbs.Clear();
        nearbySnakes.Clear();

        // Detecta objetos pr�ximos
        DetectNearbyObjects();

        // Prioridade 1: Evitar colis�es com outras cobras
        Vector3 avoidanceForce = AvoidSnakes();
        if (avoidanceForce != Vector3.zero)
        {
            direction = avoidanceForce.normalized;
            return;
        }

        // Prioridade 2: Coletar orbes pr�ximos
        Vector3 seekForce = SeekOrbs();
        if (seekForce != Vector3.zero)
        {
            direction = seekForce.normalized;
            return;
        }

        // Comportamento padr�o: Wander (vaguear inteligente)
        direction = Wander().normalized;
    }

    private void DetectNearbyObjects()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(owner.transform.position, detectionRadius);

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject != owner)
            {
                if (hitCollider.CompareTag("Orb"))
                {
                    nearbyOrbs.Add(hitCollider.gameObject);
                }
                else if (hitCollider.CompareTag("Snake"))
                {
                    nearbySnakes.Add(hitCollider.gameObject);
                }
            }
        }
    }

    private Vector3 AvoidSnakes()
    {
        if (nearbySnakes.Count == 0) return Vector3.zero;

        Vector3 avoidanceForce = Vector3.zero;
        int count = 0;

        foreach (var snake in nearbySnakes)
        {
            // Ignora o pr�prio corpo da cobra
            if (snake == owner || snake.transform.IsChildOf(owner.transform))
                continue;

            Vector3 toSnake = owner.transform.position - snake.transform.position;
            float distance = toSnake.magnitude;

            if (distance < obstacleAvoidanceDistance)
            {
                avoidanceForce += toSnake.normalized * (obstacleAvoidanceDistance - distance);
                count++;
            }
        }

        if (count > 0)
        {
            avoidanceForce /= count;
        }

        return avoidanceForce;
    }

    private Vector3 SeekOrbs()
    {
        if (nearbyOrbs.Count == 0) return Vector3.zero;

        // Encontra o orbe mais pr�ximo
        GameObject closestOrb = null;
        float closestDistance = float.MaxValue;

        foreach (var orb in nearbyOrbs)
        {
            float distance = Vector3.Distance(owner.transform.position, orb.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestOrb = orb;
            }
        }

        if (closestOrb != null)
        {
            return (closestOrb.transform.position - owner.transform.position).normalized;
        }

        return Vector3.zero;
    }

    private Vector3 Wander()
    {
        // Adiciona um pequeno desvio aleat�rio ao ponto
        randomPoint += new Vector3(
            Random.Range(-1f, 1f) * wanderJitter,
            Random.Range(-1f, 1f) * wanderJitter,
            0);

        // Normaliza e projeta para o raio do c�rculo
        randomPoint = randomPoint.normalized * wanderRadius;

        // Adiciona a dist�ncia � frente do agente
        Vector3 targetLocal = randomPoint + new Vector3(0, wanderDistance, 0);

        // Converte para coordenadas globais
        Vector3 targetWorld = owner.transform.TransformPoint(targetLocal);

        // Retorna a dire��o para o ponto de wander
        return (targetWorld - owner.transform.position).normalized;
    }

    private Vector3 GetRandomWanderPoint()
    {
        return new Vector3(
            Random.Range(-1f, 1f) * wanderRadius,
            Random.Range(-1f, 1f) * wanderRadius,
            0);
    }
}