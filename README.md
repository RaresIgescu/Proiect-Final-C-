# Primul 11 - Online Sports Store

**Primul 11** is an online store dedicated to football enthusiasts, offering a wide range of sports products—from football boots and sports nutrition to recovery gear. Built with ASP.NET MVC and ASP.NET Razor, this project features a custom-designed database, dynamic search and sorting functionalities, and robust account management.

## Project Overview

This project is a fully-functional e-commerce application that provides a dynamic and user-friendly shopping experience. Users can browse, search, and purchase products, while administrators can manage products, categories, reviews, and users.

## Key Features

- **Dynamic Product Catalog:**
  - Browse a diverse range of sports products.
  - Search by product title, description, or even within reviews.
  - Sort products by price (ascending or descending).

- **Review & Rating System:**
  - Each product displays a rating calculated as the arithmetic mean of all its reviews.
  - Products with no reviews have a default rating of 0.
  - Users can leave reviews and rate products once logged in.

- **User Authentication & Role Management:**
  - **User:** Can browse products, add items to the shopping cart, place orders, and leave reviews.
  - **Collaborator:** Has elevated privileges for content management.
  - **Admin:** Full control over site management including adding/editing products, managing categories, moderating reviews, and managing user accounts.

- **Shopping Cart & Order Management:**
  - Registered users can add products to their shopping cart.
  - Stock validation ensures availability; if insufficient, an error is displayed.
  - Users can place orders and view detailed order histories, including order totals and item details.

- **Google Maps Integration:**
  - The site integrates the Google Maps Embedded API to display the exact location of the store’s headquarters.
  - A valid API key is used to authorize map requests.

- **Custom Branding:**
  - A unique logo was designed using Canva to reinforce the brand identity of **Primul 11**.

## Technologies Used

- **Backend:** ASP.NET MVC, ASP.NET Razor
- **Database:** Custom-built database with a dedicated schema (designed and implemented in Visual Studio)
- **Frontend:** HTML, CSS, Bootstrap
- **API Integration:** Google Maps Embedded API
- **Version Control:** Git & GitHub

## Installation & Setup

1. **Clone the Repository:**
    ```bash
    git clone https://github.com/yourusername/primul11.git
    ```
2. **Open the Solution:**  
   Open the solution in Visual Studio.
3. **Configure the Database:**  
   Update the database connection string in `appsettings.json` as required.
4. **Set Up Google Maps API:**  
   Replace `YOUR_API_KEY` in the Google Maps integration code with your actual API key.
5. **Build and Run:**  
   Build the project and run it locally.

## Contributing

Contributions are welcome! Feel free to submit pull requests or open issues if you encounter any bugs or have suggestions for improvements.

## License

This project is licensed under the [MIT License](LICENSE).

---

**Primul 11** is built with passion for football and aims to deliver the best shopping experience for sports enthusiasts. Enjoy exploring and shopping!
