# Service/Repair/Upgrade Records
These three are perhaps the most important tabs in LubeLogger. They are functionally identically to one another, except for the type of records stored in them.

Service Records: These are planned/scheduled maintenance performed on the vehicle, usually on a fixed interval. Examples include oil changes, brakes, tires, spark plugs, air filter.

Repair Records: These are unplanned/unscheduled work performed on the vehicle whether due to an accident, a component breaking unexpectedly, or a broken component with no fixed maintenance interval. Examples include replacing the alternators, starters, radiators, power steering pump, bumpers.

Upgrade Records: These are work performed on the vehicle that enhances the functionality or aesthetics of the vehicle. Examples include: roof racks, lift kits, aftermarket wheels, stereos.

To add a new record, simply navigate to the tab and click the "Add New Service/Repair/Upgrade Record" button and you will be prompted to input the details of the record.

![](/Records/Service%20Records/a/image-1706797383329.png)

## Moving Records
To move existing records between the three tabs, simply click on the dropdown button to the right of the Delete button and select the tab to move the record to.

![](/Records/Service%20Records/a/image-1706797399768.png)

## Supplies Requisition
If you have supplies set up, you can click the "Choose Supplies" link under the Cost field, and a dialog will prompt you to select the supplies and quantity of each supplies you wish to requisition for this record.

![](/Records/Service%20Records/a/image-1706402198461.png)

Once you have selected the supplies, the Cost field will automatically update to reflect the costs of the supplies you have selected based on the quantity of each supply. Note that at this point, before the record is created, the supply is not requisitioned yet and you can still edit the selected supplies/quantities.

Once the record has been created, the supplies will be requisitioned and the quantity / cost of the supplies will be deducted according to the usage. This cannot be reversed(i.e.: you cannot restore the quantities by editing an existing service/repair/upgrade record), you have to go to the Supplies tab to correct the quantity/cost of the supply.

### Supplies Unit Cost Calculation
LubeLogger is not an inventory management system. Unit costs are calculated as an average of total spent / quantity, which means that everytime you replenish your supplies, it will average out the cost even if the latest batch of supplies you purchased is significantly costlier than the last batch. There is no LIFO/FIFO/FAFO inventory valuation methods.

For more information on Supplies, see [[Supplies|Records/Supplies]]

## Adding Reminders
You are given the option to set a reminder upon creating a record. This is helpful for recurring services such as Oil Changes. To do so, simply check the "Add Reminder" switch before clicking the "Add New Service Record" button. A new dialog will show up after the record has been created and all the fields will be pre-populated.

For more information on Reminders, see [[Reminders|Records/Reminders]]
