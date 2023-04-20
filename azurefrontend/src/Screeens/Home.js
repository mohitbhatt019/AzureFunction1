import axios from "axios";
import { useEffect, useState } from "react";
import { Form, json, useLocation } from "react-router-dom";
import * as signalR from "@microsoft/signalr";

import { createGlobalStyle } from "styled-components";
function Home() {
  const [file, setFile] = useState("");
  const [dataList, setDataList] = useState([]);
  const [employeeForm, setEmployeeForm] = useState({});
  const [hubConnection, setHubConnection] = useState(null);



  // Initialize the component and establish a SignalR connection
useEffect(() => {
  // Start a debugger to pause the code and allow for debugging
  debugger;

  // Retrieve the current user from localStorage
  let userData = localStorage.getItem("curruntUser");

  // Use a ternary operator to decide whether to get all data or just specific data based on the user
  {
    userData == "admin" ? getAll() : gettSpecific();
  }

  // Create a new SignalR connection and specify the URL, transport protocol, and options
  const connection = new signalR.HubConnectionBuilder()
    .withUrl("https://localhost:7180/getDataSignalR", {
      skipNegotiation: true,
      transport: signalR.HttpTransportType.WebSockets,
    })
    .build();

  // Start the SignalR connection and invoke a method to get the user ID
  connection
    .start()
    .then(() => {
      var id = localStorage.getItem("userId");
      connection.invoke("GetUserId", id);
      console.log("connection established ");
    })
    .catch(() => {
      console.log("connection not established");
    });

  // Define a method to handle updated data received via SignalR and update the state
  connection.on("SendUpdatedDataViaSignalR", (connection) => {
    setDataList(connection);
    setHubConnection(connection);
  });

  // Define a cleanup function to close the SignalR connection when the component unmounts
  return () => {
    connection.stop();
  };
}, []);




  // Function to handle the update button click
function updateClick() {
  // Start a debugger to pause the code and allow for debugging
  debugger;

  // Log the employee form data to the console
  console.log(employeeForm);

  // Make a PUT request to update the entity with the employee form data
  axios
    .put(`https://localhost:7180/updateentityasync`, employeeForm)
    .then((d) => {
      if (d.data) {
        //hubConnection.invoke("updateentityasync", employeeForm);
        console.log(d.data.data);
       // alert("Updated successfully");
      } else {
        alert("Not Updated");
      }
    })
    .catch((error) => {
      console.log(error);
      //alert("Something wrong");
    });
}



// Function to handle the file input change
const handleFileChange = (e) => {
  // Log the selected file to the console
  console.log(e.target.files);

  // Set the file state to the selected file
  setFile(e.target.files[0]);
};

// Function to handle form input changes
const changeHandler = (event) => {
  // Update the employee form data with the new input value
  setEmployeeForm({
    ...employeeForm,
    [event.target.name]: event.target.value,
  });
};



// Function to handle the file upload button click
const uploadClick = () => {
  // Get the user ID from localStorage
  const userData = localStorage.getItem("userId");

  // Create a header with the user ID
  const header = {
    userId: userData.toString(),
  };

  // Create a new FormData object and append the selected file to it
  const formData = new FormData();
  formData.append("file", file);

  // Make a POST request to upload the file with the header and form data
  axios
    .post("http://localhost:7202/api/FileUplode", formData, {
      headers: header,
    })
    .then((response) => {
      if (response) {
        alert("File saved successfully");
        //getAll();
      } else {
        alert("File not saved successfully");
      }
      console.log(response);
    })
    .catch((error) => {
      console.error(error);
    });
};



  // Function to retrieve all data from the server
function getAll() {
  //debugger;

  // Get the current user from localStorage
  let usr = localStorage.getItem("curruntUser");

  // Send a GET request to the server to retrieve all data
  axios
    .get(`https://localhost:7180/getAll`)
    .then((response) => {
      // If the server responds with data, update the state with the retrieved data
      if (response) {
        setDataList(response.data);
        console.log(response.data);
      } else {
        // If the server responds with an error, display an alert
        alert("data not recieved");
      }
    })
    .catch((error) => {
      // If there's an error, display an alert with the error message
      alert(error);
    });
}



  // Function to retrieve data for a specific user from the server
function gettSpecific() {
  //debugger

  // Get the user ID from localStorage
  let id = localStorage.getItem("userId");

  // Send a GET request to the server to retrieve data for the specific user
  axios
    .get(`https://localhost:7180/GetAllEntityForSpecificUser/${id}`)
    .then((response) => {
      // If the server responds with data, update the state with the retrieved data
      if (response) {
        setDataList(response.data);
        console.log(response.data);
      } else {
        // If the server responds with an error, display an alert
        alert("data not recieved");
      }
    })
    .catch((error) => {
      // If there's an error, display an alert with the error message
      alert(error);
    });
}


 // Function to download an image from the server
function ImageDownload(name, extension) {
  //debugger;

  // Create a filename by concatenating the name and extension
  let fileName = name + "." + extension;

  // Display the filename in an alert
  alert(fileName);

  // Send a GET request to the server to download the image
  axios
    .get("http://localhost:7202/api/DownloadImage/" + fileName, {
      responseType: "blob",
    })
    .then((response) => {
      // If the server responds with the image data, create a download link and download the image
      const url = URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement("a");
      link.href = url;
      link.setAttribute("download", fileName);
      document.body.appendChild(link);
      link.click();
    })
    .catch((error) => {
      // If there's an error, log the error to the console
      console.log(error);
    });
}


// Function to handle clicking the "Edit" button on a row of data
function editClick(data) {
  //debugger

  // Update the state with the data for the row that was clicked
  setEmployeeForm(data);
  console.log(data);
}



  function removeImage(data) {
    //debugger;
    //localhost:7180/Delete/a/a/a/a
    https: axios
      .delete(
        `https://localhost:7180/Delete/${data.name}/${data.extension}/${data.partitionKey}/${data.rowKey}`
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
    // debugger;
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
                className="btn btn-outline-primary m-2"
                type="file"
              />
              <button
                className="btn btn-outline-primary m-2"
                onClick={uploadClick}
              >
                UPLOAD
              </button>

              <button
                className="btn btn-outline-primary m-2"
                onClick={gettSpecific}
              >
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
            <th>UserId</th>
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
              <td>{data.userId}</td>
              <td>
                <button
                  className="btn btn-outline-secondary m-1"
                  onClick={() => ImageDownload(data.name, data.extension)}
                >
                  Download
                </button>
              </td>
              <td>
                <button
                  onClick={() => editClick(data)}
                  className="btn btn-outline-secondary m-1"
                  data-toggle="modal"
                  data-target="#myModal"
                >
                  Edit
                </button>
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
