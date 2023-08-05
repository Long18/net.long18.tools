# SO Browser - Custom Unity Editor Tool

The "SO Browser" is a custom editor tool developed for Unity, designed to facilitate the browsing, inspection, and management of Scriptable Objects within the Unity Editor. Scriptable Objects are a powerful feature in Unity, enabling data-driven development and easy asset management.

## Key Features:

1. **Type-Based Browsing:** The tool allows users to browse Scriptable Objects of different types. It automatically detects and displays custom editors for each Scriptable Object type, ensuring that the objects are presented appropriately with their specific properties and data.

2. **Search and Filter:** Users can search for Scriptable Objects by their names using a fuzzy matching algorithm. This enables users to quickly find specific assets, even with partially matching names.

3. **Inspect Properties:** Upon selecting a Scriptable Object, the tool presents an inspector panel displaying the object's properties. This makes it easy to review and modify the attributes of individual Scriptable Objects.

4. **History Tracking:** The tool keeps track of the inspected Scriptable Objects, allowing users to navigate back to previously viewed assets. This feature enhances workflow efficiency during browsing and inspection sessions.

5. **Create and Import:** Users can create new Scriptable Objects directly from the tool. Additionally, the tool supports importing Scriptable Objects from TSV files (Tab-Separated Values), streamlining asset management processes.

## Usage:

1. Copy the provided script files into your Unity project's script folder.

2. Define custom editors for the Scriptable Object types you want to browse. These editors should inherit from the generic class `SOBrowserEditor<>`. Custom editors can be tailored to render and inspect the properties of specific Scriptable Object types.

3. Register your custom editors by adding them to the `types` list in the `ReloadSOBrowserEditors` method.

4. To access the "SO Browser," go to `Tools -> Scriptable Object Browser` in the Unity Editor's main menu.

**Important Notes:**

- For optimal functionality, ensure that you have custom editors defined for the Scriptable Object types you wish to browse. Without appropriate editors, the Scriptable Objects may not be displayed correctly.

- The fuzzy search feature offers a flexible and forgiving way to search for Scriptable Objects, making the tool more user-friendly.

- The tool's history tracking capability improves usability by enabling users to revisit previously inspected assets during a browsing session.

- Creating new Scriptable Objects and importing them from TSV files directly through the tool streamlines the asset creation process.

- The "SO Browser" is intended for use within the Unity Editor and may not work correctly outside this environment.

**Enhance your SO management and improve your workflow with this versatile custom editor tool!**
# SO Browser - Custom Unity Editor Tool

The "SO Browser" is a custom editor tool developed for Unity, designed to facilitate the browsing, inspection, and management of Scriptable Objects within the Unity Editor. Scriptable Objects are a powerful feature in Unity, enabling data-driven development and easy asset management.

## Key Features:

1. **Type-Based Browsing:** The tool allows users to browse Scriptable Objects of different types. It automatically detects and displays custom editors for each Scriptable Object type, ensuring that the objects are presented appropriately with their specific properties and data.

2. **Search and Filter:** Users can search for Scriptable Objects by their names using a fuzzy matching algorithm. This enables users to quickly find specific assets, even with partially matching names.

3. **Inspect Properties:** Upon selecting a Scriptable Object, the tool presents an inspector panel displaying the object's properties. This makes it easy to review and modify the attributes of individual Scriptable Objects.

4. **History Tracking:** The tool keeps track of the inspected Scriptable Objects, allowing users to navigate back to previously viewed assets. This feature enhances workflow efficiency during browsing and inspection sessions.

5. **Create and Import:** Users can create new Scriptable Objects directly from the tool. Additionally, the tool supports importing Scriptable Objects from TSV files (Tab-Separated Values), streamlining asset management processes.

## Usage:

1. Copy the provided script files into your Unity project's script folder.

2. Define custom editors for the Scriptable Object types you want to browse. These editors should inherit from the generic class `SOBrowserEditor<>`. Custom editors can be tailored to render and inspect the properties of specific Scriptable Object types.

3. Register your custom editors by adding them to the `types` list in the `ReloadSOBrowserEditors` method.

4. To access the "SO Browser," go to `Tools -> SO Browser` in the Unity Editor's main menu.

**Important Notes:**

- For optimal functionality, ensure that you have custom editors defined for the Scriptable Object types you wish to browse. Without appropriate editors, the Scriptable Objects may not be displayed correctly.

- The fuzzy search feature offers a flexible and forgiving way to search for Scriptable Objects, making the tool more user-friendly.

- The tool's history tracking capability improves usability by enabling users to revisit previously inspected assets during a browsing session.

- Creating new Scriptable Objects and importing them from TSV files directly through the tool streamlines the asset creation process.

- The "SO Browser" is intended for use within the Unity Editor and may not work correctly outside this environment.

**Enhance your SO management and improve your workflow with this versatile custom editor tool!**
