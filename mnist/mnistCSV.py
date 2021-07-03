import sys
from mnist import MNIST

mndata = MNIST('ubyte')

images, labels = mndata.load_training()
images2, labels2 = mndata.load_testing()

for i in range(0,10000):
    outF = open("CSV/test/" + str(i) + ".csv", "w")
    outF.write(str(labels2[i]) + "\n")
    for x in range(0,784):
        if ((x % 28) == 27) and (x != 0):
            outF.write(str(images2[i][x]) + "\n")
        else:
            outF.write(str(images2[i][x]) + ",")
    outF.close()

for i in range(0,60000):
    outF = open("CSV/train/" + str(i) + ".csv", "w")
    outF.write(str(labels[i]) + "\n")
    for x in range(0,784):
        if ((x % 28) == 27) and (x != 0):
            outF.write(str(images[i][x]) + "\n")
        else:
            outF.write(str(images[i][x]) + ",")
    outF.close()

