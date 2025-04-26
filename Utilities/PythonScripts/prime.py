LENGTH = 1000
sieve = [True] * LENGTH
sieve[0] = sieve[1] = False
for i in range(2, LENGTH):
    if sieve[i] == True:
        for j in range(i*2, LENGTH, i): 
            sieve[j] = False
t = 0
for i in range(LENGTH):
    if sieve[i] == True:
        t = t + 1
        print(str(t) + " " + str(i))