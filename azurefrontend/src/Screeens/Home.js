import axios from "axios";
import { useEffect, useState } from "react";
import { Form, json, useLocation } from "react-router-dom";
import * as signalR from "@microsoft/signalr";

import { createGlobalStyle } from "styled-components";
function Home() {
  const [selectedFile, setSelectedFile] = useState(0);
  const [file, setFile] = useState("");



  const [dataList, setDataList] = useState([]);
  const[employeeForm,setEmployeeForm]=useState({});
  const location=useLocation();
  const [hubConnection, setHubConnection] = useState(null);
  // const [fileName, setFileName] = useState("");
  const [data, setData] = useState([]);
  // useEffect(() => {
  //   getAll();
  //   debugger
  //   const connection = new signalR.HubConnectionBuilder().withUrl("https://localhost:7180/getDataSignalR",{
  //     skipNegotiation: true,
  //     transport: signalR.HttpTransportType.WebSockets
  //   }).build()

  //   connection.start().then(() => {
  //     console.log("SignalR Hub Connection started successfully!");
  //   }).catch((error) => {
  //     console.log(`SignalR Hub Connection error: ${error}`);
  //   });

  //   connection.on("SendUpdatedDataToUser", (connection) => {
  //     debugger
  //     setData(connection);


  //   });

  // // cleanup function to close the connection when the component unmounts
  // return () => {
  //   connection.stop();
  //   setHubConnection(connection);

  // };
  // },[]);

  useEffect(()=>{

    //debugger
    // const userData = JSON.parse(localStorage.getItem("curruntUser"));
    // {userData.data.userName=="admin"?(getAll()):(gettSpecific())}
    // alert(userData.data.userName)
   gettSpecific()

    const connection = new signalR.HubConnectionBuilder().withUrl("https://localhost:7180/getDataSignalR",{
       skipNegotiation: true,
       transport: signalR.HttpTransportType.WebSockets
     }).build()
    connection
      .start()
      .then(() => {
        console.log("connection established ");
      })
      .catch(() => {
        console.log("connection not established");
      });
    connection.on("SendOffersToUser", (connection) => {

      setDataList(connection);

      setHubConnection(connection);
    });

    // cleanup function to close the connection when the component unmounts
    return () => {
      connection.stop();
    }
  },[])


  function updateClick(){
    debugger
    console.log(employeeForm)
    axios.put(`https://localhost:7180/updateentityasync`,employeeForm).then((d)=>{
      if(d){

           //hubConnection.invoke("updateentityasync", employeeForm);
           console.log(d.data)
        alert("Updated successfully");
      }
      else{
        alert("Not Updated")
      }
    }).catch((error)=>{
      console.log(error)
      alert("Something  wrongg")
    })

  }




  const handleFileChange = (e) => {
    console.log(e.target.files);
    setFile(e.target.files[0]);
  };


  const changeHandler = (event) => {
    setEmployeeForm({
      ...employeeForm,
      [event.target.name]: event.target.value,
    });
  };



  const uploadClick = () => {
    debugger
    const userData = localStorage.getItem("userId");
    const header = {
      userId: userData.toString(),
    };
    const formData = new FormData();
    formData.append("file", file);

    axios.post("http://localhost:7202/api/FileUplode", formData, {
      headers: header,
    })
      .then((response) => {
        if(response){
          alert("File save succesfully")
          //getAll();
        }

else{
  alert("File not save succesfully")

}
        console.log(response);
      })
      .catch((error) => {
        console.error(error);
      });
  }



  function getAll () {

    debugger;
    let usr = localStorage.getItem("curruntUser");


    axios
      .get(`https://localhost:7180/getAll`)

      .then((response) => {

        if (response) {
          setDataList(response.data);
          console.log(response.data);
        } else {
          alert("data not recieved");
        }
      })
      .catch((error) => {
        alert(error);
      });
  };

  // let usr = localStorage.getItem("curruntUser");



  // useEffect(() => {
  //   getAll();

  //   const connection = new signalR.HubConnectionBuilder()
  //     .withUrl("https://localhost:7180/getDataSignal")
  //     .build();

  //   connection.start().then(() => {
  //     setHubConnection(connection);
  //     let usr = localStorage.getItem("curruntUser");
  //     {usr=="admin"?(getAll()):(gettSpecific())}
  //   }).catch((error) => {
  //     console.error(error);
  //   });
  // }, []);

  // function updateClick(){
  //   debugger
  //   console.log(employeeForm)
  //   axios.put(`https://localhost:7180/updateentityasync`,employeeForm).then((d)=>{
  //     if(d){
  //       if (hubConnection) {
  //         hubConnection.invoke("getDataSignal", employeeForm);
  //         alert("Updated successfully");
  //       } else {
  //         alert("Hub connection is null or undefined");
  //       }
  //     }
  //     else{
  //       alert("Not Updated")
  //     }
  //   }).catch((error)=>{
  //     alert("Something went wrong")
  //   })
  // }



  function gettSpecific(){
    debugger
    let id =localStorage.getItem("userId");
    axios.get(`https://localhost:7180/GetAllEntityForSpecificUser/${id}`).then((response) => {


    if (response) {
      setDataList(response.data);
      console.log(response.data);
    } else {
      alert("data not recieved");
    }
  })
  .catch((error) => {
    alert(error);
  });
  }
  function ImageDownload(name, extension) {
    debugger;

    let fileName = name + "." + extension;
    alert(fileName);

    axios
      .get("http://localhost:7202/api/DownloadImage/" + fileName, {
        responseType: "blob",
      })
      .then((response) => {
        const url = URL.createObjectURL(new Blob([response.data]));
        const link = document.createElement("a");
        link.href = url;
        link.setAttribute("download", fileName);
        document.body.appendChild(link);
        link.click();
      })
      .catch((error) => {
        console.log(error);
      });
  }

  function editClick(data) {
    debugger
    setEmployeeForm(data)
    console.log(data)
  }

  const deleteData = {
    name: "",
    extension: "",
    partitionKey: "",
    rowKey: "",
  };

  function removeImage(data) {
    debugger;

    axios
      .delete(
        `https://localhost:7180/Delete?name=${data.name}&extension=${data.extension}&partitionKey=${data.partitionKey}&rowKey=${data.rowKey}`
      )
      .then((d) => {
        if (d) {
          //getAll();

          alert("Data deleted");
        } else {
          alert("Not deleted");
        }
      })
      .catch((error) => {
        alert("Error");
      });
  }

  function findImage(name, id) {
    debugger;
    axios
      .get(`https://localhost:7180/getentityasync?fileName=${name}&id=${id}`)
      .then((d) => {
        if (d) {
          alert("working");
          console.log(d.data);
        } else {
          alert("Not working");
        }
      })
      .catch((error) => {
        alert();
      });
  }



  return (
    <div>
      <div style={{ color: "red", fontSize: "16px" }}>
        <h4>CRUD operations with Azure</h4>
      </div>


        <h1 className="text-secondary text-center">Azure Data</h1>
        <div className="row">
        <form>
          <div className="col-12">
            <div className=" ">
              <input
                onChange={handleFileChange}
                name="file"
                className="btn btn-outline-primary m-2"
                type="file"
              />
              <button
                className="btn btn-outline-primary m-2"

                onClick={uploadClick}

              >
                UPLOAD
              </button>


              <button className="btn btn-outline-primary m-2" onClick={gettSpecific}>

                DISPLAY
              </button>
            </div>
          </div>
        </form>
      </div>

      <table className="table table-border table-active">
        <thead>
          <tr>
            <th>Id</th>
            <th>Name</th>
            <th>Extension</th>
            <th>Partitionkey</th>
            <th>RowKey</th>
            <th>DownLoad</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {dataList.map((data, index) => (
            <tr key={index}>
              <td>{data.id}</td>
              <td>{data.name}</td>
              <td>{data.extension}</td>
              <td>{data.partitionKey}</td>
              <td>{data.rowKey}</td>
              <td>
                <button
                  className="btn btn-outline-secondary m-1"
                  onClick={() => ImageDownload(data.name, data.extension)}
                >
                  Download
                </button>
              </td>
              <td>
              <button onClick={()=>editClick(data)} className='btn btn-outline-secondary m-1' data-toggle="modal"
                data-target="#myModal">Edit</button>
                <button
                  className="btn btn-outline-secondary m-1"
                  onClick={() => removeImage(data)}
                >
                  Delete
                </button>
                <button
                  className="btn btn-outline-secondary m-1"
                  onClick={() => findImage(data.name, data.id)}
                >
                  Details
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>

            {/*  Edit */}
            <form>
        <div className="modal" id="myModal" role="dialog">
          <div className="modal-dialog">
            <div className="modal-content">
              {/* header */}
              <div className="modal-dialog ">
                <div className="modal-content"></div>
              </div>
              {/* Body */}
              <div className="modal-body">
                <div className="form-group row">
                  <label for="txtId" className="col-sm-4">
                    Name
                  </label>
                  <div className="col-sm-8">
                    <input
                      type="text"
                      id="txtId"
                      placeholder="enter Name"
                      name="name"
                      value={employeeForm.name}
                      className="form-control"
                      onChange={changeHandler}
                    />
                  </div>
                </div>

                {/* <div className="form-group row">

                  <label for="txtname" className="col-sm-4">
                    Extension
                  </label>
                  <div className="col-sm-8">
                    <input
                      type="text"
                      id="extension"
                      name="extension"
                      placeholder="Enter Extension"
                      value={employeeForm.extension}
                      onChange={changeHandler}
                      className="form-control"
                    />
                  </div>

                </div> */}

                </div>

              {/* Footer */}
              <div className="modal-footer bg-info">
                <button
                  id="saveButton"
                  className="btn btn-success"
                  data-dismiss="modal"
                  onClick={updateClick}
                >
                  Update
                </button>
                <button className="btn btn-danger" data-dismiss="modal">
                  Cancel
                </button>
              </div>
            </div>
          </div>
        </div>
      </form>
    </div>
  );
}

export default Home;
