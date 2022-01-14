using UnityEngine;

namespace BlackHolePortalGun
{
    public class PortalGun
    {
        public OWML.Common.IModHelper Log;
        public GameObject hole1;
        public GameObject hole2;

        public void setLogger(OWML.Common.IModHelper Logger) { Log = Logger; }

        public void shoot_blackHole(Vector3 normal, Vector3 point, OWRigidbody targetRigidbody)
        {
            if (hole1)
            {
                UnityEngine.Object.Destroy(hole1);
            }

            hole1 = MakeBlackHole(1f, targetRigidbody);
            place_object(normal, point, hole1, targetRigidbody);
            if (hole2)
            {
                connect_portals();
            }
        }

        public void shoot_whiteHole(Vector3 normal, Vector3 point, OWRigidbody targetRigidbody)
        {
            if (hole2)
            {
                UnityEngine.Object.Destroy(hole2);
            }

            Sector sector = Locator._playerSectorDetector._sectorList[0];
            hole2 = MakeWhiteHole(sector, targetRigidbody, 1f);
            place_object(normal, point, hole2, targetRigidbody);

            if (hole1)
            {
                connect_portals();
            }
        }

        public void place_object(Vector3 normal, Vector3 point, GameObject gameObject, OWRigidbody targetRigidbody)
        {
            Log.Console.WriteLine($"Fired");

            Transform parent = targetRigidbody.transform;
            gameObject.SetActive(true);
            gameObject.transform.SetParent(parent);
            gameObject.transform.position = point + gameObject.transform.TransformDirection(Vector3.zero);

            if (gameObject.GetComponentInChildren<OWCollider>() != null)
            {
                gameObject.GetComponentInChildren<OWCollider>().SetActivation(true);
                gameObject.GetComponentInChildren<OWCollider>().enabled = true;
            }
        }
        public void connect_portals()
        {
            hole2.GetComponentInChildren<WhiteHoleVolume>()._radius = 0f;
            hole1.GetComponentInChildren<ModifiedBlackHoleDestructionVolume>().setWhiteHole(hole2.GetComponentInChildren<WhiteHoleVolume>());
        }

        public GameObject MakeBlackHole(float size, OWRigidbody targetRigidbody)
        {
            var blackHole = new GameObject("BlackHole");
            blackHole.SetActive(false);
            blackHole.transform.parent = targetRigidbody.transform;
            blackHole.transform.localPosition = Vector3.zero;

            var blackHoleRender = new GameObject("BlackHoleRender");
            blackHoleRender.transform.parent = blackHole.transform;
            blackHoleRender.transform.localPosition = Vector3.zero;
            blackHoleRender.transform.localScale = Vector3.one * size;

            var meshFilter = blackHoleRender.AddComponent<MeshFilter>();
            meshFilter.mesh = GameObject.Find("BrittleHollow_Body/BlackHole_BH/BlackHoleRenderer").GetComponent<MeshFilter>().mesh;

            var meshRenderer = blackHoleRender.AddComponent<MeshRenderer>();
            Shader blackHoleShader = GameObject.Find("BrittleHollow_Body/BlackHole_BH/BlackHoleRenderer").GetComponent<MeshRenderer>().sharedMaterial.shader;
            meshRenderer.material = new Material(blackHoleShader);
            meshRenderer.material.SetFloat("_Radius", size * 0.4f);
            meshRenderer.material.SetFloat("_MaxDistortRadius", size * 0.95f);
            meshRenderer.material.SetFloat("_MassScale", 1);
            meshRenderer.material.SetFloat("_DistortFadeDist", size * 0.55f);

            var destructionVolumeGO = new GameObject("DestructionVolume");
            destructionVolumeGO.layer = LayerMask.NameToLayer("BasicEffectVolume");
            destructionVolumeGO.transform.parent = blackHole.transform;
            destructionVolumeGO.transform.localScale = Vector3.one;
            destructionVolumeGO.transform.localPosition = Vector3.zero;

            var sphereCollider = destructionVolumeGO.AddComponent<SphereCollider>();
            sphereCollider.radius = size * 0.4f;
            sphereCollider.isTrigger = true;
            destructionVolumeGO.AddComponent<ModifiedBlackHoleDestructionVolume>();

            return blackHole;
        }

        private GameObject MakeWhiteHole(Sector sector, OWRigidbody OWRB, float size )
        {
            var whiteHole = new GameObject("WhiteHole");
            whiteHole.SetActive(false);
            whiteHole.transform.parent = OWRB.transform;
            whiteHole.transform.localPosition = Vector3.zero;

            var whiteHoleRenderer = new GameObject("WhiteHoleRenderer");
            whiteHoleRenderer.transform.parent = whiteHole.transform;
            whiteHoleRenderer.transform.localPosition = Vector3.zero;
            whiteHoleRenderer.transform.localScale = Vector3.one * size * 2.8f;

            var meshFilter = whiteHoleRenderer.AddComponent<MeshFilter>();
            meshFilter.mesh = GameObject.Find("WhiteHole_Body/WhiteHoleVisuals/Singularity").GetComponent<MeshFilter>().mesh;

            var meshRenderer = whiteHoleRenderer.AddComponent<MeshRenderer>();
            Shader whiteHoleShader = GameObject.Find("WhiteHole_Body/WhiteHoleVisuals/Singularity").GetComponent<MeshRenderer>().sharedMaterial.shader;
            meshRenderer.material = new Material(whiteHoleShader);
            meshRenderer.sharedMaterial.SetFloat("_Radius", size * 0.4f);
            meshRenderer.sharedMaterial.SetFloat("_DistortFadeDist", size);
            meshRenderer.sharedMaterial.SetFloat("_MaxDistortRadius", size * 2.8f);
            meshRenderer.sharedMaterial.SetColor("_Color", new Color(1.88f, 1.88f, 1.88f, 1f));

            var ambientLight = GameObject.Instantiate(GameObject.Find("WhiteHole_Body/WhiteHoleVisuals/AmbientLight_WH"));
            ambientLight.transform.parent = whiteHole.transform;
            ambientLight.transform.localScale = Vector3.one;
            ambientLight.transform.localPosition = Vector3.zero;
            ambientLight.name = "AmbientLight";
            ambientLight.GetComponent<Light>().range = size * 7f;

            var proxyShadow = sector.gameObject.AddComponent<ProxyShadowCasterSuperGroup>();

            // it's going to complain 
            GameObject whiteHoleVolumeGO = GameObject.Instantiate(GameObject.Find("WhiteHole_Body/WhiteHoleVolume"));

            whiteHoleVolumeGO.transform.parent = whiteHole.transform;
            whiteHoleVolumeGO.transform.localPosition = Vector3.zero;
            whiteHoleVolumeGO.transform.localScale = Vector3.one;
            whiteHoleVolumeGO.GetComponent<SphereCollider>().radius = size;
            whiteHoleVolumeGO.name = "WhiteHoleVolume";

            var whiteHoleFluidVolume = whiteHoleVolumeGO.GetComponent<WhiteHoleFluidVolume>();
            whiteHoleFluidVolume._innerRadius = size * 0.5f;
            whiteHoleFluidVolume._outerRadius = size;
            whiteHoleFluidVolume._attachedBody = OWRB;

            var whiteHoleVolume = whiteHoleVolumeGO.GetComponent<WhiteHoleVolume>();
            whiteHoleVolume._debrisDistMax = size * 6.5f;
            whiteHoleVolume._debrisDistMin = size * 2f;
            whiteHoleVolume._whiteHoleSector = sector;
            whiteHoleVolume._fluidVolume = whiteHoleFluidVolume;
            whiteHoleVolume._whiteHoleBody = OWRB;
            whiteHoleVolume._whiteHoleProxyShadowSuperGroup = proxyShadow;

            whiteHoleVolumeGO.GetComponent<SphereCollider>().radius = size;

            whiteHoleVolume.enabled = true;
            whiteHoleFluidVolume.enabled = false;


            return whiteHole;
        }
    }
}
