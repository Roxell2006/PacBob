using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Meduse : MonoBehaviour
{
    GameObject go_Bob;
    Animator anim_bob, anim_med;

    Vector2Int player, med, oldposition;

    Vector2 horiz = new Vector2(.48f, 0);
    Vector2 verti = new Vector2(0, .42f);
    Vector2 desti = Vector2.zero;

    [SerializeField] float speed = .04f;

    float moveX, moveY;

    int antiBlocage = 0;

    bool watchX = true;
    bool watchY = true;
    bool alternatif = true;

    // Start is called before the first frame update
    void Start()
    {
        go_Bob = GameObject.Find("bob_11");
        anim_bob = go_Bob.GetComponent<Animator>();
        anim_med = GetComponent<Animator>();
        desti = transform.position;
    }

    void FixedUpdate()
    {
        // récupère l'emplacement de bob pour voir ou se diriger
        // se diriger vers bob ou s'en éloigner si bob est en mode attaque.
        // si sur un axe on arrive à zero, on continue le mouvement jusqu'à ce que l'autre axe puisse bouger
        // la méduse doit toujours être en mouvement, si elle reste stationnaire plus de 2 tours c'est qu'elle est coincée
        // inverser une des deux directions (aléatoirement) pour la débloquer
        // se diriger alternativement d'un axe à l'autre (d'abord horizontalement puis verticalement)

        //animation de la méduse mode normal ou peur
        if (!bob.gameover)
        {
            if (anim_bob.GetBool("Attaque"))
                anim_med.SetBool("Fear", true);
            else
                anim_med.SetBool("Fear", false);

            player = converti(go_Bob.transform.position);   // position de bob
            med = converti(transform.position);             // position de la meduse

            if (watchX)
            {
                // se dirige vers bob sur l'axe X
                if (player.x < med.x)
                    moveX = -1;
                else if (player.x > med.x)
                    moveX = 1;
                else if (player.x == med.x)
                    watchX = false;

                if (anim_bob.GetBool("Attaque"))
                    moveX *= -1;
            }
            if (watchY)
            {
                // se dirige vers bob sur l'axe X
                if (player.y < med.y)
                    moveY = -1;
                else if (player.y > med.y)
                    moveY = 1;
                else if (player.y == med.y)
                    watchY = false;

                if (anim_bob.GetBool("Attaque"))
                    moveY *= -1;
            }

            // on regarde de nouveau l'axe qui a été annulé s'il y a eu une direction sur l'autre axe
            if (!watchX && med.y != oldposition.y)
                watchX = true;
            if (!watchY && med.x != oldposition.x)
                watchY = true;

            //si on est bloqué on change de direction aléatoirement
            if (antiBlocage > 2)
            {
                antiBlocage = 0;
                int rnd = Random.Range(0, 2);
                if (rnd == 1)
                {
                    // on inverse et bloque l'axe X
                    moveX *= -1;
                    watchX = false;
                }
                else
                {
                    moveY *= -1;
                    watchY = false;
                }
            }

            // Surveille si on est en mouvement
            if ((Vector2)transform.position == desti)
            {
                antiBlocage++;  // nombre de fois qu'on est stationnaire
                alternatif = !alternatif;
                moving(alternatif);
            }

            //sauvegarde de la position précédente
            oldposition = converti(transform.position);

            Vector2 p = Vector2.MoveTowards(transform.position, desti, speed);
            GetComponent<Rigidbody2D>().MovePosition(p);

            void moving(bool x)
            {
                if (x)
                {
                    if (moveX > 0 && Valid(horiz))
                    {
                        // Right
                        desti = (Vector2)transform.position + horiz;
                        antiBlocage = 0;
                    }
                    if (moveX < 0 && Valid(-horiz))
                    {
                        // Left
                        desti = (Vector2)transform.position - horiz;
                        antiBlocage = 0;
                    }
                }
                else
                {
                    if (moveY > 0 && Valid(verti))
                    {     // Up
                        desti = (Vector2)transform.position + verti;
                        antiBlocage = 0;
                    }

                    if (moveY < 0 && Valid(-verti))
                    {     // Down
                        desti = (Vector2)transform.position - verti;
                        antiBlocage = 0;
                    }
                }
            }
        }   
    }

    bool Valid(Vector2 dir)
    {
        Vector2 pos = transform.position;
        RaycastHit2D hit = Physics2D.Linecast(pos + dir, pos);
        Debug.DrawRay(pos, dir, Color.red);

        return (hit.collider == GetComponent<Collider2D>() || hit.collider.name == "Item" || hit.collider.name== "bob_11");
    }

    Vector2Int converti(Vector2 pos)
    {
        Vector2Int position = new Vector2Int();
        position.y = (int)Mathf.Floor(pos.y / .42f);
        position.x = (int)Mathf.Floor(pos.x / .48f);
        return position;
    }

    private void OnDisable()
    {
        //réinitialiser la destination quand la méduse se fait attraper
        desti = transform.position;
    }
}
