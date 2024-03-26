using Unity.VisualScripting;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace toio.VisualScript
{
    [UnitTitle("Visual Scripting Connect Cube")]
    [UnitCategory("t4U\\Connecter")]
    public class VisualScriptingConnectCubeUnit : Unit
    {
        [DoNotSerialize] // No need to serialize ports.
        public ControlInput inputTrigger; //Adding the ControlInput port variable

        [DoNotSerialize] // No need to serialize ports.
        public ControlOutput outputTrigger;//Adding the ControlOutput port variable.

        [DoNotSerialize] // No need to serialize ports
        public ValueInput Connecttype; // Adding the ValueInput variable for Connecttype

        [DoNotSerialize] // No need to serialize ports
        public ValueInput ConnectNum; // Adding the ValueInput variable for ConnectNum

        [DoNotSerialize] // No need to serialize ports
        public ValueOutput result_cubemanager, result_cube, result_cubelist; // Adding the ValueOutput variable for result

        private CubeManager _cubemanager;

        private List<Cube> _cubelist;

        private Cube _cube;

        protected override void Definition() //The method to set what our node will be doing.
        {
            //The lambda to execute our node action when the inputTrigger port is triggered.
            inputTrigger = ControlInputCoroutine("inputTrigger", WaitConnection);
            outputTrigger = ControlOutput("outputTrigger");

            Connecttype = ValueInput<ConnectType>("ConnectType", ConnectType.Auto);
            ConnectNum = ValueInput<int>("ConnectNum", 1);

            result_cube = ValueOutput<Cube>("Cube", (flow) => { return _cube; });
            result_cubelist = ValueOutput<List<Cube>>("CubeList", (flow) => { return _cubelist; });
            result_cubemanager = ValueOutput<CubeManager>("CubeManager", (flow) => { return _cubemanager; });

            Requirement(Connecttype, inputTrigger); //To display that we need the ConnectType value to be set to let the unit process
            Requirement(ConnectNum, inputTrigger); //To display that we need the ConnectNum value to be set to let the unit process
            Succession(inputTrigger, outputTrigger); //To display that the input trigger port input will exits at the output trigger port exit. Not setting your succession also grays out the connected nodes but the execution is still done.

            //To display the data that is written when the inputTrigger is triggered to the result output.
            Assignment(inputTrigger, result_cube);
            Assignment(inputTrigger, result_cubelist);
            Assignment(inputTrigger, result_cubemanager);
        }

        private async void ConnectCube(CubeManager cubemanager, int connectnum)
        {
            await cubemanager.MultiConnect(connectnum);
        }

        private async void ConnectCube(CubeManager cubemanager)
        {
            _cube = await cubemanager.SingleConnect();
        }

        private IEnumerator WaitConnection(Flow flow)
        {
            // Get Value from port
            int connectnum = flow.GetValue<int>(ConnectNum);
            ConnectType connecttype = flow.GetValue<ConnectType>(Connecttype);

            // Connect Cubes with CubeManager
            CubeManager cubemanager = new CubeManager(connecttype);
            if (connectnum == 1)
            {
                this.ConnectCube(cubemanager);
            }
            else
            {
                this.ConnectCube(cubemanager, connectnum);
            }

            // Wait unitl num of connected cubes equals 'connectnum'
            List<Cube> cubes = cubemanager.cubes;
            yield return new WaitUntil(() => cubes.Count == connectnum);

            // Set output variables
            _cubemanager = cubemanager;
            _cubelist = _cubemanager.cubes;
            _cube = _cubelist[0];

            yield return outputTrigger;
        }
    }
}