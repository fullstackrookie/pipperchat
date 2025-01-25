import React from "react";
import { Link } from "react-router-dom";

const WelcomePage = () => {
   const navigate   =   useNavigate();

    return (
        <div    className="WelcomePage">
            {/*Welcome Message */}
            <div    className="WelcomeMessage">
                <h1>Welcome</h1>
        </div>
        {/* Action  Buttons */}
        <div    className="ActionButtons">
            <button 
                className="btn-signin"
                onClick={() => navigate('/signin')}>
                Sign In
            </button>
            <button 
                className="btn-signup"
                onClick={() => navigate('/signup')}>
                Sign Up
            </button>
        </div>
    </div>
    );
};

export default WelcomePage;