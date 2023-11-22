# ClockIn
**ClockIn** - Shift Organizer with Face Detection

ClockIn is a comprehensive shift organizer designed to streamline workforce management, featuring a user-friendly interface and advanced face detection capabilities utilizing Emgu.CV with the Eigen Face recognition algorithm.

## **Key Features**

### **Clock In Window**
Detects and displays all available cameras, allowing users to select the desired one.
Enables workers to seamlessly clock in/out through face detection technology.
Users can set a maximum shift time, ensuring adherence to predefined limits.

### **Manager Login Window**
Provides a simple login interface for managers, with easily configurable code stored in the config file.

### **Add Worker Window**
Allows for the addition of new workers to the system.
Face recognition ensures accuracy, and users must input essential details such as first name, last name, and a unique 9-digit ID.
Enforces naming conventions, requiring capitalization for first and last names.

### **Remove Worker Window**
Displays a list of all workers, facilitating the easy removal of selected individuals.

### **Report Window**
Generates PDF reports detailing worker hours for a specified month.
Users can choose the desired month from a list of recorded months.

### **Monthly Database Management**
Automatically creates a new LiteDB, a NoSQL database, for each new month of the year.
Transfers worker data and resets hours for the new month while preserving the previous month's records.

### **Technologies Used**
Emgu.CV with Eigen Face Recognition
Utilizes the Emgu.CV library for efficient face detection and recognition.
Implements the Eigen Face algorithm for accurate facial identification.
LiteDB Employs LiteDB, a NoSQL database, for flexible and scalable data management.
Seamlessly integrates with the system, providing robust storage capabilities.
WinUI3 for User Interface Features a visually appealing UI powered by WinUI3, enhancing the overall user experience.

### **Project Structure**
The project is organized into distinct windows, each catering to specific functionalities such as clocking in/out, manager login, worker management, and reporting. The underlying technologies ensure reliable face detection, secure database management, and an intuitive user interface.

ClockIn is a versatile solution designed to optimize shift organization, combining cutting-edge facial recognition with an elegant user interface for an efficient and user-friendly experience.
