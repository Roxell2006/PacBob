using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public class bob : MonoBehaviour
{
    Animator anim;
    AudioSource audioS;

    [SerializeField] AudioClip snd_Epuisette, snd_Laugh, snd_Food, snd_die;

    Vector2 horiz = new Vector2(.48f, 0);
    Vector2 verti = new Vector2(0, .42f);
    Vector2 desti = Vector2.zero;

    Vector2[] meduse = new Vector2[3];

    float speed = .06f;
    bool lookRight = true;
    static int level = 1;
    int[] food = new int[] {124, 148, 146};
    int time, interval; // timer

    public static bool gameover = false;
    public Score score;
    protected Joystick joystick;

    // Start is called before the first frame update
    void Start()
    {
        time = (int)Time.time;
        audioS = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();
        desti = transform.position;

        meduse[0] = new Vector2(-3.1f, 1.05f);
        meduse[1] = new Vector2(-3.1f, 2.73f);
        meduse[2] = new Vector2(-1.18f, 1.05f);

        gameover = false;
        if (level == 1)
            Score.score = 0;
        score.AfficheScore();
        joystick = FindObjectOfType<Joystick>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!gameover)
        {
            time = (int)Time.time;
            if (interval == time)
                anim.SetBool("Attaque", false);

            float moveX, moveY;

            //controle au joystick virtuel
            moveX = joystick.Horizontal > .2f ? 1 : joystick.Horizontal < -.2f ? -1 : 0;
           moveY = joystick.Vertical   > .2f ? 1 : joystick.Vertical   < -.2f ? -1 : 0;

            //Controle au clavier et au Joystick
            if(Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0 || Mathf.Abs(Input.GetAxisRaw("Vertical"))>0)
            {
                moveX = Input.GetAxisRaw("Horizontal");
                moveY = Input.GetAxisRaw("Vertical");
            }

            if ((Vector2)transform.position == desti)
            {
                if (moveY > 0 && Valid(verti))      // Up
                    desti = (Vector2)transform.position + verti;
                if (moveX > 0 && Valid(horiz))      // Right
                    desti = (Vector2)transform.position + horiz;
                if (moveY < 0 && Valid(-verti))     // Down
                    desti = (Vector2)transform.position - verti;
                if (moveX < 0 && Valid(-horiz))     // Left
                    desti = (Vector2)transform.position - horiz;
            }

            Vector2 p = Vector2.MoveTowards(transform.position, desti, speed);
            GetComponent<Rigidbody2D>().MovePosition(p);

            if (moveX > 0 && !lookRight)
                Flip();
            else if (moveX < 0 && lookRight)
                Flip();

            //Speed vérifie si on est en mouvement pour l'animation
            if (Mathf.Abs(moveX) > 0)
                anim.SetFloat("Speed", Mathf.Abs(moveX));
            else if (Mathf.Abs(moveY) > 0)
                anim.SetFloat("Speed", Mathf.Abs(moveY));
            else
                anim.SetFloat("Speed", 0);
        }
    }
     
    bool Valid(Vector2 dir)
    {
        Vector2 pos = transform.position;
        RaycastHit2D hit = Physics2D.Linecast(pos + dir, pos);
        Debug.DrawRay(pos, dir, Color.red);

        if(hit.collider.gameObject.GetComponent<Tilemap>() != null)
            EraseItem(hit.collider.gameObject.GetComponent<Tilemap>(), pos + dir);
        
        return (hit.collider == GetComponent<Collider2D>() || 
            hit.collider.name == "Item" ||
            hit.collider.name == "meduse_0" ||
            hit.collider.name == "meduse_1" ||
            hit.collider.name == "meduse_2" 
        );
    }
    void EraseItem(Tilemap map, Vector2 pos)
    {
        if(map.name == "Item")
        {
            //convertir un Vector2 en vector3Int
            pos.y = Mathf.Floor(pos.y / .42f);
            pos.x = Mathf.Floor(pos.x / .48f);

            //efface une tuile
            map.SetTile(new Vector3Int((int)pos.x, (int)pos.y, 0), null);

            //detect si c'est une tuile épuisette ou paté de crabe
            if( pos.x == -18 && pos.y == 10 || pos.x == -18 && pos.y == -12
                || pos.x == 7 && pos.y == 10 || pos.x == 7 && pos.y == -12)
            {
                //epuisette
                audioS.PlayOneShot(snd_Epuisette);
                anim.SetBool("Attaque", true);
                interval = (int)Time.time + 8;  // 8 sec
            }
            else
            {
                //paté de crabe
                audioS.PlayOneShot(snd_Food);
                food[level-1]--;
                Score.score++;
                score.AfficheScore();
                if(food[level-1] == 0) // si plus de paté de crabe on a gagné
                {
                    audioS.PlayOneShot(snd_Laugh);
                    anim.SetTrigger("Win");
                    gameover = true;
                    level++;
                    if(level > 3)
                    {
                        level = 1;
                        StartCoroutine(GoTo("Menu"));
                    }  
                    else
                        StartCoroutine(GoTo("level" + level));        
                }
            }
        }
    }

    void Flip()
    {
        lookRight = !lookRight;
        Vector2 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.name.Substring(0,3) == "med")
        {
            if (!anim.GetBool("Attaque"))
            {
                // si bob n'est pas en mode attaque on est mort
                audioS.PlayOneShot(snd_die);
                anim.SetTrigger("Die");
                gameover = true;
                level = 1;
                StartCoroutine(GoTo("Menu"));
            }
            else
            {
                // sinon on capture la meduse
                collision.transform.position = meduse[int.Parse(collision.name.Substring(7, 1))];
                collision.gameObject.SetActive(false);
                audioS.PlayOneShot(snd_Laugh);
                Score.score += 10;
                score.AfficheScore();
                // coroutine qui permet de réactiver la méduse après 2 secondes
                StartCoroutine(Repop(collision.gameObject));
            }
        }
    }

    IEnumerator Repop(GameObject objet)
    {
        yield return new WaitForSeconds(2);
        objet.SetActive(true);
    }
    IEnumerator GoTo(string scene)
    {
        yield return new WaitForSeconds(3);
        SceneManager.LoadScene(scene);
    }
}
