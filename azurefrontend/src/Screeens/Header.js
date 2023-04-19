import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";

function Header() {
  const [user, setUser] = useState(null);
  const navigate = useNavigate();

  useEffect(() => {
    let usr = localStorage.getItem("curruntUser");
    setUser(usr);
  });
  
  function logoutClick() {
    localStorage.clear();
    alert("logout successfull");
    navigate("/login");
  }
  return (
    <div>
      <nav class="navbar navbar-expand-lg navbar-light bg-light">
        <div class="container-fluid">
          <Link to="/home" class="nav-link active" aria-current="page" href="#">
            Home
          </Link>
          <button
            class="navbar-toggler"
            type="button"
            data-bs-toggle="collapse"
            data-bs-target="#navbarSupportedContent"
            aria-controls="navbarSupportedContent"
            aria-expanded="false"
            aria-label="Toggle navigation"
          >
            <span class="navbar-toggler-icon"></span>
          </button>
          <div
            class="collapse navbar-collapse justify-content-end"
            id="navbarSupportedContent"
          >
            <ul class="navbar-nav">
              <li class="nav-item"></li>
              {user ? (
                <a
                  onClick={logoutClick}
                  class="btn btn-outline-success my-2 my-sm-0"
                  type="submit"
                >
                  Logout
                </a>
              ) : (
                <li class="nav-item">
                  <Link
                    to="login"
                    class="nav-link btn btn-outline-success my-2 my-sm-0 m-2"
                    href="#"
                  >
                    Login
                  </Link>
                </li>
              )}
            </ul>
          </div>
        </div>
      </nav>
    </div>
  );
}

export default Header;
