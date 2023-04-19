import axios from "axios";
import { useState } from "react";
import { useNavigate } from "react-router-dom";

function Login() {
  const initData = {
    Username: "",
    Password: "",
  };

  const [formData, setFormData] = useState();
  const changeHandler = (event) => {
    setFormData({
      ...formData,
      [event.target.name]: event.target.value,
    });
  };
  const navigate = new useNavigate();

  function loginClick() {
    debugger;
    console.log(formData);

    //alert("Work");
    axios.get(`https://localhost:7180/login/${formData.Username}/${formData.Password}`)
      .then((d) => {
        if (d) {
          debugger
          console.log(d.data);
          console.log(d.data.data.userName);
          console.log(d.data.data.id);
          localStorage.setItem("curruntUser", d.data.data.userName);
          localStorage.setItem("userId",d.data.data.id)

          navigate("/home");
          alert(d.data.message);
        } else {
          alert("Wrong username/Password");
        }
      })
      .catch((err) => {
        alert("Something went wrong");
        console.log(err);
      });
  }

  return (
    <div>
      <div className="row col-lg-6 mx-auto m-2 p-2">
        <div class="card text-center">
          <div class="card-header text-info">Login</div>
          <div class="card-body">
            <div className="form-group row">
              <label className="col-lg-4" for="txtusername">
                UserName
              </label>
              <div className="col-lg-8">
                <input
                  type="text"
                  id="txtusername"
                  onChange={changeHandler}
                  placeholder="Enter Username"
                  className="Form-control"
                  name="Username"

                />
              </div>
            </div>
            <div className="form-group row">
              <label className="col-lg-4" for="txtpassword">
                Password
              </label>
              <div className="col-lg-8">
                <input
                  type="password"
                  id="txtpassword"
                  onChange={changeHandler}
                  placeholder="Enter Password"
                  className="Form-control"
                  name="Password"

                />
              </div>
            </div>
          </div>
          <div className="card-footer text-muted">
            <button onClick={loginClick} className="btn btn-info">
              Login
            </button>
          </div>
        </div>
      </div>{" "}
    </div>
  );
}

export default Login;
