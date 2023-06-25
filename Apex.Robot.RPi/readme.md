# Hardware setup instructions

dtoverlay=i2c-gpio,bus=4,i2c_gpio_delay_us=1,i2c_gpio_sda=27,i2c_gpio_scl=22
dtoverlay=i2c-gpio,bus=3,i2c_gpio_delay_us=1,i2c_gpio_sda=25,i2c_gpio_scl=24

sudo i2cdetect -l

You will now see that i2c bus 3 and 4 is also listed. Also run:

sudo i2cdetect -y 3
sudo i2cdetect -y 4

i2c bus 0 - reserved
i2c bus 1 - illuminance
i2c bus 2 - reserved
i2c bus 3 - infrared
i2c bus 4 - temperature/humidity

