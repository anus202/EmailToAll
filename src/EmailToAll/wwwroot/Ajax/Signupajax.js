$("#SignupForm").on("submit", function (e) {
    e.preventDefault();

    const signupData = {
        Email: $("#Email").val(),
        Password: $("#Password").val(),
        ConfirmPass: $("#ConfirmPass").val()
    };

    $.ajax({
        url: "/Signup/SignupForm",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(signupData),
        success: function (response) {
            if (response.success) {
                // The API may return the token as `Token` or `token` depending on serialization settings.
                const jwtToken = response.data.Token || response.data.token;

                if (jwtToken) {
                    // Keep the JWT in localStorage for subsequent requests.
                    localStorage.setItem("authToken", jwtToken);
                }

                // Remember the email for the OTP verification screen.
                localStorage.setItem("userEmail", signupData.Email);

                window.location.href = "/Signup/OtpPage";
            } else {
                alert(response.message || "Signup failed!");
            }
        },
        error: function (xhr, status, error) {
            console.error("Error:", error);
            alert("An error occurred during signup.");
        }
    });
});
